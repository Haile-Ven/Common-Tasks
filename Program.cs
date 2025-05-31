using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Common_Tasks
{
    static class Program
    {
        private static readonly string MutexName = "Common Tasks";

        [STAThread]
        static void Main()
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, MutexName, out createdNew))
            {
                if (createdNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    
                    // Initialize AppConfig to ensure the log directory is created
                    string logPath = AppConfig.LogFilePath;
                    
                    // Register for messages from other instances
                    AppMessageHandler.RegisterMessageHandler();
                    
                    MainForm mainForm = new MainForm();
                    Application.Run(mainForm);
                }
                else
                {
                    // Find and activate the existing instance instead of showing an error
                    ActivateExistingInstance();
                    
                    // Exit this instance
                    return;
                }
            }
        }
        
        /// <summary>
        /// Finds and activates the existing instance of the application
        /// </summary>
        private static void ActivateExistingInstance()
        {
            // Try to find the main window of the existing instance
            // and send a message to bring it to the foreground
            AppMessageHandler.SendMessageToExistingInstance();
            
            // Pass any command line arguments to the existing instance if needed
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                // If we have the --clear-events argument, send it to the existing instance
                if (args.Contains("--clear-events"))
                {
                    AppMessageHandler.SendClearEventsCommand();
                }
            }
        }
    }
}