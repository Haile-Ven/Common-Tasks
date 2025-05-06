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
            try
            {
                // Initialize the database path in the installation folder
                string installationFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                
                // Set the database path
                DatabasePath = Path.Combine(installationFolder, "shutdown.db");
                
                // Test if we can write to this location
                TestDatabaseAccess(installationFolder);
            }
            catch (UnauthorizedAccessException)
            {
                // Fallback to user's AppData folder if we don't have write access to the installation folder
                string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Common Tasks");
                
                // Create the directory if it doesn't exist
                if (!Directory.Exists(appDataFolder))
                {
                    Directory.CreateDirectory(appDataFolder);
                }
                
                // Set the database path to the AppData folder
                DatabasePath = Path.Combine(appDataFolder, "shutdown.db");
            }
            
            // Set the connection string after the database path has been determined
            ConnectionString = $"Data Source={DatabasePath};Version=3;";
            
            // Initialize the database if it doesn't exist
            InitializeDatabase();
        }
        
        /// <summary>
        /// Initializes the SQLite database and creates necessary tables if they don't exist
        /// </summary>
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
                        // Create the shutdown_schedule table
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
        
        /// <summary>
        /// Saves the shutdown schedule to the database
        /// </summary>
        /// <param name="startTime">The time when the shutdown was scheduled</param>
        /// <param name="shutdownTime">The time when the system will shut down</param>
        /// <returns>True if the operation was successful</returns>
        public static bool SaveShutdownSchedule(DateTime startTime, DateTime shutdownTime)
        {
            try
            {
                // First, deactivate any existing active schedules
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
        
        /// <summary>
        /// Gets the active shutdown schedule from the database
        /// </summary>
        /// <returns>Tuple containing start time and shutdown time, or null if no active schedule</returns>
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
        
        /// <summary>
        /// Deletes all active shutdown schedules from the database
        /// </summary>
        /// <returns>True if the operation was successful</returns>
        public static bool DeleteAllActiveSchedules()
        {
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    
                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = "DELETE FROM shutdown_schedule WHERE is_active = 1";
                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting schedules: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Deactivates all shutdown schedules in the database (kept for backward compatibility)
        /// </summary>
        /// <returns>True if the operation was successful</returns>
        public static bool DeactivateAllSchedules()
        {
            // Now we just call DeleteAllActiveSchedules for consistency
            return DeleteAllActiveSchedules();
        }
        
        /// <summary>
        /// Deletes expired shutdown schedules from the database
        /// </summary>
        /// <returns>True if the operation was successful</returns>
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
                            UPDATE shutdown_schedule 
                            SET is_active = 0 
                            WHERE is_active = 1 AND datetime(shutdown_time) <= datetime('now', 'localtime')";
                        
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
        
        /// <summary>
        /// Tests if the application has write access to the specified folder
        /// </summary>
        /// <param name="folderPath">The folder to test</param>
        /// <exception cref="UnauthorizedAccessException">Thrown if the application doesn't have write access</exception>
        private static void TestDatabaseAccess(string folderPath)
        {
            try
            {
                // Try to create a temporary file to test write access
                string testFile = Path.Combine(folderPath, "write_test.tmp");
                File.WriteAllText(testFile, "Test");
                File.Delete(testFile);
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Re-throw to be caught by the caller
            }
            catch (IOException ex)
            {
                // If it's an access issue, convert to UnauthorizedAccessException
                if (ex.Message.Contains("access") || ex.Message.Contains("denied"))
                {
                    throw new UnauthorizedAccessException("Cannot write to the installation folder", ex);
                }
                // Otherwise just log it but continue
                Console.WriteLine($"IO error testing database access: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log other exceptions but continue
                Console.WriteLine($"Error testing database access: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if there is an active shutdown schedule
        /// </summary>
        /// <returns>True if there is an active schedule</returns>
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
