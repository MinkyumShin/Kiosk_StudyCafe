using System;
using System.Data.SQLite;
using System.IO;

namespace Kiosk_StudyCafe
{
    public class UserManager
    {
        private readonly string dbPath = "StudyCafe.sqlite";
        private readonly string connectionString;

        public string CurrentUserId { get; private set; }
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
                        CreatedAt DATETIME
                    )";

                using (var cmd = new SQLiteCommand(createUsersQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool Register(string userId, string password, string name)
        {
            return false;
        }


        //테스트
        public bool Login(string userId, string password)
        {
            if (userId == "admin" && password == "1234")
            {
                CurrentUserId = userId;
                return true;
            }

            return false;
        }


        /* DB 연결 로그인
        public bool Login(string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT COUNT(*) 
                    FROM Users 
                    WHERE UserId = @UserId AND Password = @Password";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Password", password);

                    long count = (long)cmd.ExecuteScalar();

                    if (count > 0)
                    {
                        CurrentUserId = userId;
                        return true;
                    }

                    return false;
                }
            }
        }
        */
        public void Logout()
        {
            CurrentUserId = null;
        }

        public string GetCurrentUser()
        {
            return CurrentUserId;
        }
    }
}