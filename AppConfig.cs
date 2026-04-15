using System;
using System.IO;

namespace Common_Tasks
{
    public static class AppConfig
    {
        private static string _logFilePath;

        static AppConfig()
        {
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Common Tasks");

            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            _logFilePath = Path.Combine(appDataFolder, "shutdown.log");
        }

        public static string LogFilePath => _logFilePath;
    }
}
