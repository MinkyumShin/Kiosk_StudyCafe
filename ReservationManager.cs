using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Kiosk_StudyCafe
{
    public class ReservationManager
    {
        private readonly string dbPath = "StudyCafe.sqlite";
        private readonly string connectionString;

        public ReservationManager()
        {
            connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        // DB 테이블 생성 및 초기 좌석 데이터 세팅
        private void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // 좌석 테이블
                string createSeatsQuery = @"
                    CREATE TABLE IF NOT EXISTS Seats (
                        SeatNumber INTEGER PRIMARY KEY,
                        Type VARCHAR(20)
                    )";

                // 예약 테이블
                string createResQuery = @"
                    CREATE TABLE IF NOT EXISTS Reservations (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId VARCHAR(50),
                        SeatNumber INTEGER,
                        ReservationDate VARCHAR(20),
                        StartTime DATETIME,
                        DurationHours INTEGER,
                        FOREIGN KEY(SeatNumber) REFERENCES Seats(SeatNumber)
                    )";

                using (var cmd = new SQLiteCommand(createSeatsQuery, conn)) cmd.ExecuteNonQuery();
                using (var cmd = new SQLiteCommand(createResQuery, conn)) cmd.ExecuteNonQuery();

                // 1~60번 좌석이 DB에 없으면 초기화 (1~50: Single, 51~60: Group)
                using (var checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM Seats", conn))
                {
                    long count = (long)checkCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        using (var transaction = conn.BeginTransaction())
                        {
                            string insertQuery = "INSERT INTO Seats (SeatNumber, Type) VALUES (@SeatNumber, @Type)";
                            for (int i = 1; i <= 60; i++)
                            {
                                using (var insertCmd = new SQLiteCommand(insertQuery, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@SeatNumber", i);
                                    insertCmd.Parameters.AddWithValue("@Type", i <= 50 ? "Single" : "Group");
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                            transaction.Commit();
                        }
                    }
                }
            }
        }

        // 특정 날짜의 특정 좌석이 예약되어 있는지 확인 (데이터 무결성 검증)
        public bool IsSeatAvailable(int seatNumber, string date)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Reservations WHERE SeatNumber = @SeatNumber AND ReservationDate = @Date";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    cmd.Parameters.AddWithValue("@Date", date);
                    long count = (long)cmd.ExecuteScalar();
                    return count == 0;
                }
            }
        }

        // 예약 트랜잭션 처리 (원자성 보장)
        public bool ReserveSeat(string userId, int seatNumber, string date, int hours)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. 한번 더 빈 좌석인지 검증 (방어적 프로그래밍)
                        string checkQuery = "SELECT COUNT(*) FROM Reservations WHERE SeatNumber = @SeatNumber AND ReservationDate = @Date";
                        using (var checkCmd = new SQLiteCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                            checkCmd.Parameters.AddWithValue("@Date", date);
                            long count = (long)checkCmd.ExecuteScalar();

                            if (count > 0)
                            {
                                transaction.Rollback();
                                return false; // 이미 예약됨
                            }
                        }

                        // 2. 예약 데이터 삽입
                        string insertQuery = @"
                            INSERT INTO Reservations (UserId, SeatNumber, ReservationDate, StartTime, DurationHours) 
                            VALUES (@UserId, @SeatNumber, @Date, @StartTime, @Hours)";

                        using (var insertCmd = new SQLiteCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@UserId", userId);
                            insertCmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                            insertCmd.Parameters.AddWithValue("@Date", date);
                            insertCmd.Parameters.AddWithValue("@StartTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            insertCmd.Parameters.AddWithValue("@Hours", hours);
                            insertCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // 특정 날짜의 모든 예약된 좌석 번호 가져오기 (UI 초기 렌더링용)
        public List<int> GetReservedSeats(string date)
        {
            List<int> reservedSeats = new List<int>();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT SeatNumber FROM Reservations WHERE ReservationDate = @Date";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservedSeats.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
            return reservedSeats;
        }

        // 1분마다 실행되어 시간이 만료된 예약이나 노쇼를 처리하는 메서드
        public void ProcessTimeoutsAndNoShows()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. 이용 시간이 완전히 종료된 예약 내역 자동 삭제
                        string expireQuery = @"
                            DELETE FROM Reservations 
                            WHERE datetime(StartTime, '+' || DurationHours || ' hours') <= datetime('now', 'localtime')";

                        using (var cmd = new SQLiteCommand(expireQuery, conn, transaction))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("타이머 DB 처리 오류: " + ex.Message);
                    }
                }
            }
        }
    }
}