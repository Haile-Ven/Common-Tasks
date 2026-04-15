using System;
using System.Data.SQLite;
using System.IO;

namespace Common_Tasks
{
    public static class DatabaseManager
    {
        private static readonly string DatabasePath;
        private static readonly string ConnectionString;

        static DatabaseManager()
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Common Tasks");

            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            DatabasePath = Path.Combine(appDataFolder, "shutdown.db");

            ConnectionString = $"Data Source={DatabasePath};Version=3;";

            InitializeDatabase();
        }

        private static void InitializeDatabase()
        {
            bool newDatabase = !File.Exists(DatabasePath);

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                if (newDatabase)
                {
                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS shutdown_schedule (
                                id INTEGER PRIMARY KEY AUTOINCREMENT,
                                start_time TEXT NOT NULL,
                                shutdown_time TEXT NOT NULL,
                                is_active INTEGER NOT NULL DEFAULT 1,
                                created_at TEXT NOT NULL
                            )";
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static bool SaveShutdownSchedule(DateTime startTime, DateTime shutdownTime)
        {
            try
            {
                DeactivateAllSchedules();

                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = @"
                            INSERT INTO shutdown_schedule 
                                (start_time, shutdown_time, is_active, created_at) 
                            VALUES 
                                (@startTime, @shutdownTime, 1, @createdAt)";

                        command.Parameters.AddWithValue("@startTime", startTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@shutdownTime", shutdownTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving shutdown schedule: {ex.Message}");
                return false;
            }
        }

        public static Tuple<DateTime, DateTime> GetActiveShutdownSchedule()
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = @"
                            SELECT start_time, shutdown_time 
                            FROM shutdown_schedule 
                            WHERE is_active = 1 
                            ORDER BY id DESC 
                            LIMIT 1";

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                DateTime startTime = DateTime.Parse(reader["start_time"].ToString());
                                DateTime shutdownTime = DateTime.Parse(reader["shutdown_time"].ToString());

                                return new Tuple<DateTime, DateTime>(startTime, shutdownTime);
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting active shutdown schedule: {ex.Message}");
                return null;
            }
        }

        public static bool DeleteAllActiveSchedules()
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new SQLiteCommand(connection))
                            {
                                command.CommandText = "DELETE FROM shutdown_schedule";
                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception("Failed to delete schedules: " + ex.Message, ex);
                        }
                    }

                    using (var command = new SQLiteCommand("VACUUM", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                string errorLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CommonTasks_Error.log");
                File.AppendAllText(errorLogPath, $"Error deleting schedules: {ex.Message}\r\n");
                return false;
            }
        }

        public static bool DeactivateAllSchedules()
        {
            return DeleteAllActiveSchedules();
        }

        public static bool DeleteExpiredSchedules()
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = @"
                            DELETE FROM shutdown_schedule 
                            WHERE datetime(shutdown_time) <= datetime('now', 'localtime')";

                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting expired schedules: {ex.Message}");
                return false;
            }
        }


        public static bool HasActiveSchedule()
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = @"
                            SELECT COUNT(*) 
                            FROM shutdown_schedule 
                            WHERE is_active = 1";

                        long count = (long)command.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for active schedule: {ex.Message}");
                return false;
            }
        }
    }
}
