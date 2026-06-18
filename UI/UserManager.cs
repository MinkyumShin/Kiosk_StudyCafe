using System;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kiosk_StudyCafe
{
    public class UserManager
    {
        private readonly string dbPath = "StudyCafe.sqlite";
        private readonly string connectionString;

        public string? CurrentUserId { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(CurrentUserId);

        public UserManager()
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

                string createUsersQuery = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        UserId VARCHAR(50) PRIMARY KEY,
                        Password VARCHAR(100),
                        Name VARCHAR(50),
                        Phone VARCHAR(20),
                        RRN VARCHAR(10),
                        Points INTEGER DEFAULT 0,
                        CumulativeHours INTEGER DEFAULT 0,
                        CreatedAt DATETIME
                    )";

                using (var cmd = new SQLiteCommand(createUsersQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 기존 DB 호환용 컬럼 보강
                TryAlter(conn, "ALTER TABLE Users ADD COLUMN Phone VARCHAR(20)");
                TryAlter(conn, "ALTER TABLE Users ADD COLUMN RRN VARCHAR(10)");
                TryAlter(conn, "ALTER TABLE Users ADD COLUMN Points INTEGER DEFAULT 0");
                TryAlter(conn, "ALTER TABLE Users ADD COLUMN CumulativeHours INTEGER DEFAULT 0");
                TryAlter(conn, "ALTER TABLE Users ADD COLUMN CreatedAt DATETIME");
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

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public bool IsIdDuplicate(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return true;

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT COUNT(*) FROM Users WHERE UserId = @UserId";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId.Trim());
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public bool Register(string userId, string password, string name, string phone, string rrn)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(rrn))
            {
                return false;
            }

            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string duplicateQuery = "SELECT COUNT(*) FROM Users WHERE UserId = @UserId";
                            using (var duplicateCmd = new SQLiteCommand(duplicateQuery, conn, transaction))
                            {
                                duplicateCmd.Parameters.AddWithValue("@UserId", userId.Trim());
                                long count = (long)duplicateCmd.ExecuteScalar();

                                if (count > 0)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            string query = @"
                                INSERT INTO Users 
                                    (UserId, Password, Name, Phone, RRN, Points, CumulativeHours, CreatedAt)
                                VALUES 
                                    (@UserId, @Password, @Name, @Phone, @RRN, 10000, 0, @CreatedAt)";

                            using (var cmd = new SQLiteCommand(query, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserId", userId.Trim());
                                cmd.Parameters.AddWithValue("@Password", HashPassword(password));
                                cmd.Parameters.AddWithValue("@Name", name.Trim());
                                cmd.Parameters.AddWithValue("@Phone", phone.Trim());
                                cmd.Parameters.AddWithValue("@RRN", rrn.Trim());
                                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public bool Login(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
                return false;

            userId = userId.Trim();

            // 발표/테스트용 기본 계정 유지
            if ((userId == "admin" && password == "1234") ||
                (userId == "A" && password == "123"))
            {
                CurrentUserId = userId;
                return true;
            }

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT Password FROM Users WHERE UserId = @UserId";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    object? result = cmd.ExecuteScalar();

                    if (result != null && result.ToString() == HashPassword(password))
                    {
                        CurrentUserId = userId;
                        return true;
                    }
                }
            }

            return false;
        }

        public void Logout()
        {
            CurrentUserId = null;
        }

        public string? GetCurrentUser()
        {
            return CurrentUserId;
        }

        public bool UserExists(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT COUNT(*) FROM Users WHERE UserId = @UserId";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId.Trim());
                    return (long)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public int GetUserPoints(string userId)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT Points FROM Users WHERE UserId = @UserId";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    object? result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public bool AddPoints(string userId, int points)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = "UPDATE Users SET Points = Points + @Points WHERE UserId = @UserId";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Points", points);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeductPoints(string userId, int points)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string checkQuery = "SELECT Points FROM Users WHERE UserId = @UserId";
                using (var checkCmd = new SQLiteCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@UserId", userId);
                    object? result = checkCmd.ExecuteScalar();

                    if (result == null || Convert.ToInt32(result) < points)
                        return false;
                }

                string updateQuery = "UPDATE Users SET Points = Points - @Points WHERE UserId = @UserId";
                using (var updateCmd = new SQLiteCommand(updateQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@Points", points);
                    updateCmd.Parameters.AddWithValue("@UserId", userId);
                    return updateCmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public void AddCumulativeHours(string userId, int hours)
        {
            if (string.IsNullOrWhiteSpace(userId) || hours <= 0)
                return;

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                int beforeHours = 0;
                string beforeQuery = "SELECT CumulativeHours FROM Users WHERE UserId = @UserId";
                using (var beforeCmd = new SQLiteCommand(beforeQuery, conn))
                {
                    beforeCmd.Parameters.AddWithValue("@UserId", userId);
                    object? result = beforeCmd.ExecuteScalar();
                    if (result != null)
                        beforeHours = Convert.ToInt32(result);
                }

                string updateQuery = "UPDATE Users SET CumulativeHours = CumulativeHours + @Hours WHERE UserId = @UserId";
                using (var cmd = new SQLiteCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Hours", hours);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }

                int afterHours = beforeHours + hours;

                // 10시간 단위 누적 달성 횟수 차이만큼 보너스 지급
                int beforeBonusCount = beforeHours / 10;
                int afterBonusCount = afterHours / 10;
                int bonusCount = afterBonusCount - beforeBonusCount;

                if (bonusCount > 0)
                {
                    int bonusPoints = bonusCount * 2000;
                    string bonusQuery = "UPDATE Users SET Points = Points + @Bonus WHERE UserId = @UserId";
                    using (var bonusCmd = new SQLiteCommand(bonusQuery, conn))
                    {
                        bonusCmd.Parameters.AddWithValue("@Bonus", bonusPoints);
                        bonusCmd.Parameters.AddWithValue("@UserId", userId);
                        bonusCmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}