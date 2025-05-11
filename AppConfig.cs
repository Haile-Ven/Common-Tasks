using System;
using System.IO;

namespace Common_Tasks
{
    public static class AppConfig
    {
        private static string _logFilePath;
        
        static AppConfig()
        {
            // Always use AppData folder for log storage, same as DatabaseManager
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Common Tasks");
            
            // Create the directory if it doesn't exist
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }
            
            // Set the log file path to the AppData folder
            _logFilePath = Path.Combine(appDataFolder, "shutdown.log");
        }
        
        /// <summary>
        /// Gets the full path to the log file
        /// </summary>
        public static string LogFilePath => _logFilePath;
    }
}
