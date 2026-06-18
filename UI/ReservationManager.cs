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
        public string ReservedHours { get; set; } = "";
        public string Status { get; set; } = "";

        public string DisplayText => $"[{Status}] {FormatHoursText()}";

        public List<int> GetHoursList()
        {
            List<int> list = new List<int>();

            if (!string.IsNullOrWhiteSpace(ReservedHours))
            {
                foreach (string h in ReservedHours.Split(','))
                {
                    if (int.TryParse(h.Trim(), out int hour))
                        list.Add(hour);
                }
            }

            return list.Distinct().OrderBy(h => h).ToList();
        }

        private string FormatHoursText()
        {
            List<int> hours = GetHoursList();

            if (hours.Count == 0)
                return "시간 정보 없음";

            if (hours.Count == 1)
                return $"{hours[0]:D2}:00~{hours[0] + 1:D2}:00";

            return $"{hours.First():D2}:00~{hours.Last() + 1:D2}:00";
        }
    }

    public class ReservationManager
    {
        private const int SeatCount = 68;
        private const int SingleSeatCount = 48;
        private const int PricePerHour = 2000;

        private readonly string dbPath = "StudyCafe.sqlite";
        private readonly string connectionString;

        private readonly string[] activeStatuses = { "예약됨", "입실", "외출" };

        public ReservationManager()
        {
            connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string createSeatsQuery = @"
                    CREATE TABLE IF NOT EXISTS Seats (
                        SeatNumber INTEGER PRIMARY KEY,
                        Type VARCHAR(20),
                        Status VARCHAR(20) DEFAULT '정상'
                    )";

                string createResQuery = @"
                    CREATE TABLE IF NOT EXISTS Reservations (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId VARCHAR(50),
                        SeatNumber INTEGER,
                        ReservationDate VARCHAR(20),
                        StartTime DATETIME,
                        EndTime DATETIME,
                        DurationHours INTEGER,
                        ReservedHours VARCHAR(100),
                        PaymentAmount INTEGER DEFAULT 0,
                        Status VARCHAR(20) DEFAULT '예약됨',
                        CreatedAt DATETIME,
                        FOREIGN KEY(SeatNumber) REFERENCES Seats(SeatNumber)
                    )";

                using (var cmd = new SQLiteCommand(createSeatsQuery, conn)) cmd.ExecuteNonQuery();
                using (var cmd = new SQLiteCommand(createResQuery, conn)) cmd.ExecuteNonQuery();

                // 기존 DB 호환용 컬럼 보강
                TryAlter(conn, "ALTER TABLE Seats ADD COLUMN Status VARCHAR(20) DEFAULT '정상'");
                TryAlter(conn, "ALTER TABLE Reservations ADD COLUMN EndTime DATETIME");
                TryAlter(conn, "ALTER TABLE Reservations ADD COLUMN ReservedHours VARCHAR(100)");
                TryAlter(conn, "ALTER TABLE Reservations ADD COLUMN PaymentAmount INTEGER DEFAULT 0");
                TryAlter(conn, "ALTER TABLE Reservations ADD COLUMN Status VARCHAR(20) DEFAULT '예약됨'");
                TryAlter(conn, "ALTER TABLE Reservations ADD COLUMN CreatedAt DATETIME");

                EnsureSeats(conn);
            }
        }

        private void TryAlter(SQLiteConnection conn, string query)
        {
            try
            {
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // 이미 컬럼이 있으면 무시
            }
        }

        private void EnsureSeats(SQLiteConnection conn)
        {
            using (var checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM Seats", conn))
            {
                long count = (long)checkCmd.ExecuteScalar();

                if (count >= SeatCount)
                    return;
            }

            using (var transaction = conn.BeginTransaction())
            {
                string insertQuery = @"
                    INSERT OR IGNORE INTO Seats (SeatNumber, Type, Status)
                    VALUES (@SeatNumber, @Type, '정상')";

                for (int i = 1; i <= SeatCount; i++)
                {
                    using (var insertCmd = new SQLiteCommand(insertQuery, conn, transaction))
                    {
                        insertCmd.Parameters.AddWithValue("@SeatNumber", i);
                        insertCmd.Parameters.AddWithValue("@Type", i <= SingleSeatCount ? "Single" : "Group");
                        insertCmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        private bool IsActiveStatus(string status)
        {
            return activeStatuses.Contains(status);
        }

        private string ActiveStatusSql()
        {
            return "'예약됨','입실','외출'";
        }

        private List<int> ParseReservedHours(string? reservedHours)
        {
            List<int> result = new List<int>();

            if (string.IsNullOrWhiteSpace(reservedHours))
                return result;

            foreach (string h in reservedHours.Split(','))
            {
                if (int.TryParse(h.Trim(), out int hour))
                    result.Add(hour);
            }

            return result.Distinct().OrderBy(h => h).ToList();
        }

        public List<int> GetReservedHours(int seatNumber, string date)
        {
            List<int> reservedHours = new List<int>();

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = $@"
                    SELECT ReservedHours
                    FROM Reservations
                    WHERE SeatNumber = @SeatNumber
                      AND ReservationDate = @Date
                      AND Status IN ({ActiveStatusSql()})";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    cmd.Parameters.AddWithValue("@Date", date);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string reservedText = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            reservedHours.AddRange(ParseReservedHours(reservedText));
                        }
                    }
                }
            }

            return reservedHours.Distinct().OrderBy(h => h).ToList();
        }

        public List<int> GetReservedSeats(string date)
        {
            List<int> reservedSeats = new List<int>();

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string blockQuery = "SELECT SeatNumber FROM Seats WHERE Status = '점검중'";
                using (var blockCmd = new SQLiteCommand(blockQuery, conn))
                {
                    using (var reader = blockCmd.ExecuteReader())
                    {
                        while (reader.Read())
                            reservedSeats.Add(reader.GetInt32(0));
                    }
                }

                string query = $@"
                    SELECT DISTINCT SeatNumber
                    FROM Reservations
                    WHERE ReservationDate = @Date
                      AND Status IN ({ActiveStatusSql()})";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", date);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int seatNumber = reader.GetInt32(0);

                            if (!reservedSeats.Contains(seatNumber))
                                reservedSeats.Add(seatNumber);
                        }
                    }
                }
            }

            return reservedSeats;
        }

        public bool IsSeatAvailable(int seatNumber, string date)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string statusQuery = "SELECT Status FROM Seats WHERE SeatNumber = @SeatNumber";
                using (var statusCmd = new SQLiteCommand(statusQuery, conn))
                {
                    statusCmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    object? statusResult = statusCmd.ExecuteScalar();

                    if (statusResult != null && statusResult.ToString() == "점검중")
                        return false;
                }

                string query = $@"
                    SELECT COUNT(*)
                    FROM Reservations
                    WHERE SeatNumber = @SeatNumber
                      AND ReservationDate = @Date
                      AND Status IN ({ActiveStatusSql()})";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    cmd.Parameters.AddWithValue("@Date", date);

                    return (long)cmd.ExecuteScalar() == 0;
                }
            }
        }

        public bool ReserveSeat(string userId, int seatNumber, string date, List<int> selectedHours)
        {
            if (string.IsNullOrWhiteSpace(userId) || selectedHours == null || selectedHours.Count == 0)
                return false;

            List<int> normalizedHours = selectedHours
                .Distinct()
                .Where(h => h >= 0 && h <= 23)
                .OrderBy(h => h)
                .ToList();

            if (normalizedHours.Count == 0)
                return false;

            int totalHours = normalizedHours.Count;
            int totalPrice = totalHours * PricePerHour;

            DateTime reservationDate = DateTime.Parse(date).Date;
            DateTime actualStartTime = reservationDate.AddHours(normalizedHours.First());
            DateTime actualEndTime = reservationDate.AddHours(normalizedHours.Last() + 1);

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string seatStatusQuery = "SELECT Status FROM Seats WHERE SeatNumber = @SeatNumber";
                        using (var seatStatusCmd = new SQLiteCommand(seatStatusQuery, conn, transaction))
                        {
                            seatStatusCmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                            object? statusResult = seatStatusCmd.ExecuteScalar();

                            if (statusResult != null && statusResult.ToString() == "점검중")
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }

                        // 예약 시간 충돌 검사
                        List<int> alreadyReserved = new List<int>();

                        string reservedQuery = $@"
                            SELECT ReservedHours
                            FROM Reservations
                            WHERE SeatNumber = @SeatNumber
                              AND ReservationDate = @Date
                              AND Status IN ({ActiveStatusSql()})";

                        using (var reservedCmd = new SQLiteCommand(reservedQuery, conn, transaction))
                        {
                            reservedCmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                            reservedCmd.Parameters.AddWithValue("@Date", date);

                            using (var reader = reservedCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string reservedText = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                    alreadyReserved.AddRange(ParseReservedHours(reservedText));
                                }
                            }
                        }

                        if (normalizedHours.Intersect(alreadyReserved).Any())
                        {
                            transaction.Rollback();
                            return false;
                        }

                        // 실제 가입 회원이면 포인트 차감
                        // admin, A 테스트 계정은 Users 테이블에 없을 수 있으므로 포인트 차감 없이 테스트 가능하게 둠
                        bool userExists = false;

                        string userCheckQuery = "SELECT COUNT(*) FROM Users WHERE UserId = @UserId";
                        using (var userCheckCmd = new SQLiteCommand(userCheckQuery, conn, transaction))
                        {
                            userCheckCmd.Parameters.AddWithValue("@UserId", userId);
                            userExists = (long)userCheckCmd.ExecuteScalar() > 0;
                        }

                        if (userExists)
                        {
                            string pointQuery = "SELECT Points FROM Users WHERE UserId = @UserId";
                            using (var pointCmd = new SQLiteCommand(pointQuery, conn, transaction))
                            {
                                pointCmd.Parameters.AddWithValue("@UserId", userId);
                                object? res = pointCmd.ExecuteScalar();

                                if (res == null || Convert.ToInt32(res) < totalPrice)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            string deductQuery = "UPDATE Users SET Points = Points - @Cost WHERE UserId = @UserId";
                            using (var deductCmd = new SQLiteCommand(deductQuery, conn, transaction))
                            {
                                deductCmd.Parameters.AddWithValue("@Cost", totalPrice);
                                deductCmd.Parameters.AddWithValue("@UserId", userId);
                                deductCmd.ExecuteNonQuery();
                            }
                        }

                        string insertQuery = @"
                            INSERT INTO Reservations 
                                (UserId, SeatNumber, ReservationDate, StartTime, EndTime, DurationHours, ReservedHours, PaymentAmount, Status, CreatedAt)
                            VALUES 
                                (@UserId, @SeatNumber, @Date, @StartTime, @EndTime, @Hours, @ReservedHours, @PaymentAmount, '예약됨', @CreatedAt)";

                        using (var insertCmd = new SQLiteCommand(insertQuery, conn, transaction))
                        {
                            insertCmd.Parameters.AddWithValue("@UserId", userId);
                            insertCmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                            insertCmd.Parameters.AddWithValue("@Date", date);
                            insertCmd.Parameters.AddWithValue("@StartTime", actualStartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            insertCmd.Parameters.AddWithValue("@EndTime", actualEndTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            insertCmd.Parameters.AddWithValue("@Hours", totalHours);
                            insertCmd.Parameters.AddWithValue("@ReservedHours", string.Join(",", normalizedHours));
                            insertCmd.Parameters.AddWithValue("@PaymentAmount", totalPrice);
                            insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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

                string query = $@"
                    UPDATE Reservations
                    SET Status = @Status
                    WHERE UserId = @UserId
                      AND SeatNumber = @SeatNumber
                      AND ReservationDate = @Date
                      AND Status IN ({ActiveStatusSql()})";

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

        public List<ReservationInfo> GetMyReservations(string userId, int seatNumber, string date)
        {
            List<ReservationInfo> list = new List<ReservationInfo>();

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = $@"
                    SELECT Id, ReservedHours, Status
                    FROM Reservations
                    WHERE UserId = @UserId
                      AND SeatNumber = @SeatNumber
                      AND ReservationDate = @Date
                      AND Status IN ({ActiveStatusSql()})
                    ORDER BY Id DESC";

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

        public bool IsMyReservation(string userId, int seatNumber, string date)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = $@"
                    SELECT COUNT(*)
                    FROM Reservations
                    WHERE UserId = @UserId
                      AND SeatNumber = @SeatNumber
                      AND ReservationDate = @Date
                      AND Status IN ({ActiveStatusSql()})";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    cmd.Parameters.AddWithValue("@Date", date);

                    return (long)cmd.ExecuteScalar() > 0;
                }
            }
        }

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

        public void CancelReservationById(int id)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string selectQuery = @"
                            SELECT UserId, DurationHours, PaymentAmount, Status
                            FROM Reservations
                            WHERE Id = @Id";

                        string userId = "";
                        int durationHours = 0;
                        int paymentAmount = 0;
                        string status = "";

                        using (var selectCmd = new SQLiteCommand(selectQuery, conn, transaction))
                        {
                            selectCmd.Parameters.AddWithValue("@Id", id);

                            using (var reader = selectCmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    transaction.Rollback();
                                    return;
                                }

                                userId = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                durationHours = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                                paymentAmount = reader.IsDBNull(2) ? durationHours * PricePerHour : reader.GetInt32(2);
                                status = reader.IsDBNull(3) ? "" : reader.GetString(3);
                            }
                        }

                        bool userExists = false;
                        string userCheckQuery = "SELECT COUNT(*) FROM Users WHERE UserId = @UserId";
                        using (var userCheckCmd = new SQLiteCommand(userCheckQuery, conn, transaction))
                        {
                            userCheckCmd.Parameters.AddWithValue("@UserId", userId);
                            userExists = (long)userCheckCmd.ExecuteScalar() > 0;
                        }

                        // 예약 전 취소라면 전액 환불
                        if (userExists && status == "예약됨" && paymentAmount > 0)
                        {
                            string refundQuery = "UPDATE Users SET Points = Points + @Refund WHERE UserId = @UserId";
                            using (var refundCmd = new SQLiteCommand(refundQuery, conn, transaction))
                            {
                                refundCmd.Parameters.AddWithValue("@Refund", paymentAmount);
                                refundCmd.Parameters.AddWithValue("@UserId", userId);
                                refundCmd.ExecuteNonQuery();
                            }
                        }

                        // 입실/외출 상태에서 퇴실이면 누적 시간 반영
                        if (userExists && (status == "입실" || status == "외출") && durationHours > 0)
                        {
                            string updateHourQuery = "UPDATE Users SET CumulativeHours = CumulativeHours + @Hours WHERE UserId = @UserId";
                            using (var hourCmd = new SQLiteCommand(updateHourQuery, conn, transaction))
                            {
                                hourCmd.Parameters.AddWithValue("@Hours", durationHours);
                                hourCmd.Parameters.AddWithValue("@UserId", userId);
                                hourCmd.ExecuteNonQuery();
                            }
                        }

                        string updateQuery = "UPDATE Reservations SET Status = '퇴실' WHERE Id = @Id";
                        using (var updateCmd = new SQLiteCommand(updateQuery, conn, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@Id", id);
                            updateCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void CancelReservation(string userId, int seatNumber, string date)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = $@"
                    UPDATE Reservations
                    SET Status = '퇴실'
                    WHERE UserId = @UserId
                      AND SeatNumber = @SeatNumber
                      AND ReservationDate = @Date
                      AND Status IN ({ActiveStatusSql()})";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ProcessTimeoutsAndNoShows()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                ProcessNoShows(conn);
                ProcessCompletedReservations(conn);
            }
        }

        private void ProcessNoShows(SQLiteConnection conn)
        {
            List<Tuple<int, string, int>> noShowReservations = new List<Tuple<int, string, int>>();

            string selectNoShowQuery = @"
                SELECT Id, UserId, PaymentAmount
                FROM Reservations
                WHERE Status = '예약됨'
                  AND datetime(StartTime, '+30 minutes') <= datetime('now', 'localtime')";

            using (var cmd = new SQLiteCommand(selectNoShowQuery, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string userId = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        int paymentAmount = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

                        noShowReservations.Add(new Tuple<int, string, int>(id, userId, paymentAmount));
                    }
                }
            }

            foreach (var reservation in noShowReservations)
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateStatusQuery = "UPDATE Reservations SET Status = '노쇼' WHERE Id = @Id";
                        using (var cmd = new SQLiteCommand(updateStatusQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", reservation.Item1);
                            cmd.ExecuteNonQuery();
                        }

                        // 노쇼 시 결제 포인트 50% 환불
                        int refundPoints = reservation.Item3 / 2;

                        if (!string.IsNullOrWhiteSpace(reservation.Item2) && refundPoints > 0)
                        {
                            string userCheckQuery = "SELECT COUNT(*) FROM Users WHERE UserId = @UserId";
                            bool userExists;

                            using (var userCheckCmd = new SQLiteCommand(userCheckQuery, conn, transaction))
                            {
                                userCheckCmd.Parameters.AddWithValue("@UserId", reservation.Item2);
                                userExists = (long)userCheckCmd.ExecuteScalar() > 0;
                            }

                            if (userExists)
                            {
                                string refundQuery = "UPDATE Users SET Points = Points + @Refund WHERE UserId = @UserId";
                                using (var cmd = new SQLiteCommand(refundQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@Refund", refundPoints);
                                    cmd.Parameters.AddWithValue("@UserId", reservation.Item2);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }
        }

        private void ProcessCompletedReservations(SQLiteConnection conn)
        {
            List<Tuple<int, string, int>> completedReservations = new List<Tuple<int, string, int>>();

            string selectCompletedQuery = @"
                SELECT Id, UserId, DurationHours
                FROM Reservations
                WHERE Status IN ('입실', '외출')
                  AND datetime(EndTime) <= datetime('now', 'localtime')";

            using (var cmd = new SQLiteCommand(selectCompletedQuery, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string userId = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        int hours = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

                        completedReservations.Add(new Tuple<int, string, int>(id, userId, hours));
                    }
                }
            }

            foreach (var reservation in completedReservations)
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateStatusQuery = "UPDATE Reservations SET Status = '이용완료' WHERE Id = @Id";
                        using (var cmd = new SQLiteCommand(updateStatusQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", reservation.Item1);
                            cmd.ExecuteNonQuery();
                        }

                        string userCheckQuery = "SELECT COUNT(*) FROM Users WHERE UserId = @UserId";
                        bool userExists;

                        using (var userCheckCmd = new SQLiteCommand(userCheckQuery, conn, transaction))
                        {
                            userCheckCmd.Parameters.AddWithValue("@UserId", reservation.Item2);
                            userExists = (long)userCheckCmd.ExecuteScalar() > 0;
                        }

                        if (userExists && reservation.Item3 > 0)
                        {
                            string updateHourQuery = "UPDATE Users SET CumulativeHours = CumulativeHours + @Hours WHERE UserId = @UserId";
                            using (var hourCmd = new SQLiteCommand(updateHourQuery, conn, transaction))
                            {
                                hourCmd.Parameters.AddWithValue("@Hours", reservation.Item3);
                                hourCmd.Parameters.AddWithValue("@UserId", reservation.Item2);
                                hourCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }
        }

        public bool CheckIn(int reservationId)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = "UPDATE Reservations SET Status = '입실' WHERE Id = @Id AND Status IN ('예약됨', '외출')";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", reservationId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool SetSeatMaintenance(int seatNumber, bool isMaintenance)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string status = isMaintenance ? "점검중" : "정상";
                string query = "UPDATE Seats SET Status = @Status WHERE SeatNumber = @SeatNumber";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool ForceCheckout(int seatNumber)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    UPDATE Reservations
                    SET Status = '강제퇴실'
                    WHERE SeatNumber = @SeatNumber
                      AND Status IN ('예약됨', '입실', '외출')";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}