using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace DigitalLibrary
{
    public static class DatabaseHelper
    {
        private const string DbFileName = "DigitalLibrary.sqlite";
        public static string ConnectionString = $"Data Source={DbFileName};Version=3;";

        public static void InitializeDatabase()
        {
            bool createTables = false;
            if (!File.Exists(DbFileName))
            {
                SQLiteConnection.CreateFile(DbFileName);
                createTables = true;
            }

            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Books (
                        BookId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Author TEXT NOT NULL,
                        Category TEXT,
                        AvailableCopies INTEGER NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Users (
                        UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Email TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS IssuedBooks (
                        IssueId INTEGER PRIMARY KEY AUTOINCREMENT,
                        BookId INTEGER NOT NULL,
                        UserId INTEGER NOT NULL,
                        IssueDate DATETIME NOT NULL,
                        ReturnDate DATETIME NULL,
                        LateFee NUMERIC NOT NULL DEFAULT 0,
                        FOREIGN KEY (BookId) REFERENCES Books(BookId),
                        FOREIGN KEY (UserId) REFERENCES Users(UserId)
                    );

                    CREATE TABLE IF NOT EXISTS BorrowRecords (
                        RecordId INTEGER PRIMARY KEY AUTOINCREMENT,
                        BookId INTEGER NOT NULL,
                        BookTitle TEXT NOT NULL,
                        BorrowerName TEXT NOT NULL,
                        Copies INTEGER NOT NULL,
                        BorrowTime DATETIME NOT NULL,
                        ReturnTime DATETIME NOT NULL,
                        Status TEXT NOT NULL,
                        ChargedAmount NUMERIC NOT NULL DEFAULT 0,
                        FOREIGN KEY (BookId) REFERENCES Books(BookId)
                    );
                ";
                cmd.ExecuteNonQuery();

                if (createTables)
                {
                    cmd.CommandText = "INSERT INTO Users (Name, Email) VALUES ('John Doe', 'john@user.com'), ('Jane Smith', 'jane@user.com')";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataTable ExecuteQuery(string query, params SQLiteParameter[] parameters)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    using (var adapter = new SQLiteDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static int ExecuteNonQuery(string query, params SQLiteParameter[] parameters)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
