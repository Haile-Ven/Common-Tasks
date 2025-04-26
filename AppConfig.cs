using System;
using System.IO;

namespace Common_Tasks
{
    public static class AppConfig
    {
        private static string _logFilePath;
        
        static AppConfig()
        {
            // Initialize the log file path in the Documents folder
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appFolder = Path.Combine(documentsFolder, "Common Tasks");
            
            // Create the directory if it doesn't exist
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            // Set the log file path
            _logFilePath = Path.Combine(appFolder, "shutdown.log");
        }
        
        /// <summary>
        /// Gets the full path to the log file
        /// </summary>
        public static string LogFilePath => _logFilePath;
    }
}
