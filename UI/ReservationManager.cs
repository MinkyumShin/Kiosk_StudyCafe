using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Kiosk_StudyCafe
{
    public class ReservationInfo
    {
        public int Id { get; set; }
        public string ReservedHours { get; set; }
        public string Status { get; set; }

        public string DisplayText => $"[{Status}] {ReservedHours}시";

        // ★ [추가] 문자열로 된 시간대를 int 리스트로 변환하는 도우미 메서드
        public List<int> GetHoursList()
        {
            List<int> list = new List<int>();
            if (!string.IsNullOrEmpty(ReservedHours))
            {
                foreach (var h in ReservedHours.Split(','))
                {
                    if (int.TryParse(h, out int hour)) list.Add(hour);
                }
            }
            return list;
        }
    }

    public class ReservationManager
    {
        private readonly string dbPath = "StudyCafe.sqlite";
        private readonly string connectionString;

        public ReservationManager()
        {
            connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(dbPath)) SQLiteConnection.CreateFile(dbPath);

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string createSeatsQuery = "CREATE TABLE IF NOT EXISTS Seats (SeatNumber INTEGER PRIMARY KEY, Type VARCHAR(20))";
                string createResQuery = @"
                    CREATE TABLE IF NOT EXISTS Reservations (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId VARCHAR(50),
                        SeatNumber INTEGER,
                        ReservationDate VARCHAR(20),
                        StartTime DATETIME,
                        DurationHours INTEGER,
                        ReservedHours VARCHAR(100),
                        Status VARCHAR(20) DEFAULT '예약됨'
                    )";

                using (var cmd = new SQLiteCommand(createSeatsQuery, conn)) cmd.ExecuteNonQuery();
                using (var cmd = new SQLiteCommand(createResQuery, conn)) cmd.ExecuteNonQuery();

                // 기존 DB가 있다면 ReservedHours 컬럼 추가 (에러 나면 이미 있는 것)
                try
                {
                    using (var alterCmd = new SQLiteCommand("ALTER TABLE Reservations ADD COLUMN Status VARCHAR(20) DEFAULT '예약됨'", conn)) { alterCmd.ExecuteNonQuery(); }
                }
                catch { }

                using (var checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM Seats", conn))
                {
                    long count = (long)checkCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        using (var transaction = conn.BeginTransaction())
                        {
                            string insertQuery = "INSERT INTO Seats (SeatNumber, Type) VALUES (@SeatNumber, @Type)";
                            for (int i = 1; i <= 68; i++) // 좌석 수 68개로 맞춤
                            {
                                using (var insertCmd = new SQLiteCommand(insertQuery, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@SeatNumber", i);
                                    insertCmd.Parameters.AddWithValue("@Type", i <= 48 ? "Single" : "Group");
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                            transaction.Commit();
                        }
                    }
                }
            }
        }

        // 특정 좌석의 특정 날짜 예약된 '시간대 목록' 가져오기
        public List<int> GetReservedHours(int seatNumber, string date)
        {
            List<int> reservedHours = new List<int>();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ReservedHours FROM Reservations WHERE SeatNumber = @SeatNumber AND ReservationDate = @Date";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    cmd.Parameters.AddWithValue("@Date", date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                string[] hours = reader.GetString(0).Split(',');
                                foreach (var h in hours)
                                {
                                    if (int.TryParse(h, out int hour)) reservedHours.Add(hour);
                                }
                            }
                        }
                    }
                }
            }
            return reservedHours.Distinct().ToList();
        }

        // 특정 날짜에 '예약이 하나라도 있는' 좌석 번호 가져오기
        public List<int> GetReservedSeats(string date)
        {
            List<int> reservedSeats = new List<int>();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT DISTINCT SeatNumber FROM Reservations WHERE ReservationDate = @Date";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) reservedSeats.Add(reader.GetInt32(0));
                    }
                }
            }
            return reservedSeats;
        }

        // 예약 트랜잭션 (시간대 리스트를 통째로 받도록 수정)
        public bool ReserveSeat(string userId, int seatNumber, string date, List<int> selectedHours)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 해당 시간에 다른 사람이 방금 예약했는지 검증 (교집합 확인)
                        var alreadyReserved = GetReservedHours(seatNumber, date);
                        if (selectedHours.Intersect(alreadyReserved).Any())
                        {
                            transaction.Rollback();
                            return false;
                        }

                        string insertQuery = @"
                            INSERT INTO Reservations (UserId, SeatNumber, ReservationDate, StartTime, DurationHours, ReservedHours, Status) 
                            VALUES (@UserId, @SeatNumber, @Date, @StartTime, @Hours, @ReservedHours, '예약됨')";

                        using (var insertCmd = new SQLiteCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@UserId", userId);
                            insertCmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                            insertCmd.Parameters.AddWithValue("@Date", date);
                            insertCmd.Parameters.AddWithValue("@StartTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            insertCmd.Parameters.AddWithValue("@Hours", selectedHours.Count);
                            insertCmd.Parameters.AddWithValue("@ReservedHours", string.Join(",", selectedHours));
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

        public void UpdateReservationStatus(string userId, int seatNumber, string date, string status)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Reservations SET Status = @Status WHERE UserId = @UserId AND SeatNumber = @SeatNumber AND ReservationDate = @Date";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ★ [추가] 특정 좌석에 대한 내 예약 '목록' 전체 가져오기
        public List<ReservationInfo> GetMyReservations(string userId, int seatNumber, string date)
        {
            List<ReservationInfo> list = new List<ReservationInfo>();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Id, ReservedHours, Status FROM Reservations WHERE UserId = @UserId AND SeatNumber = @SeatNumber AND ReservationDate = @Date";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    cmd.Parameters.AddWithValue("@Date", date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ReservationInfo
                            {
                                Id = reader.GetInt32(0),
                                ReservedHours = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                Status = reader.IsDBNull(2) ? "" : reader.GetString(2)
                            });
                        }
                    }
                }
            }
            return list;
        }

        // ★ [수정] 좌석+날짜 기준이 아닌, 고유 ID 기준으로 퇴실(삭제)
        public void CancelReservationById(int id)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("DELETE FROM Reservations WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ★ [수정] 고유 ID 기준으로 상태(입실/외출) 업데이트
        public void UpdateReservationStatusById(int id, string status)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("UPDATE Reservations SET Status = @Status WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ProcessTimeoutsAndNoShows()
        {
            // 기존 로직 유지
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string expireQuery = "DELETE FROM Reservations WHERE datetime(StartTime, '+' || DurationHours || ' hours') <= datetime('now', 'localtime')";
                        using (var cmd = new SQLiteCommand(expireQuery, conn, transaction)) cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch { transaction.Rollback(); }
                }
            }
        }

        // 본인의 예약인지 확인
        public bool IsMyReservation(string userId, int seatNumber, string date)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Reservations WHERE UserId = @UserId AND SeatNumber = @SeatNumber AND ReservationDate = @Date";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    cmd.Parameters.AddWithValue("@Date", date);
                    return (long)cmd.ExecuteScalar() > 0;
                }
            }
        }

        // 퇴실 시 예약 삭제
        public void CancelReservation(string userId, int seatNumber, string date)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Reservations WHERE UserId = @UserId AND SeatNumber = @SeatNumber AND ReservationDate = @Date";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}