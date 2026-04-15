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

                    string logPath = AppConfig.LogFilePath;

                    AppMessageHandler.RegisterMessageHandler();

                    MainForm mainForm = new MainForm();
                    Application.Run(mainForm);
                }
                else
                {
                    ActivateExistingInstance();

                    return;
                }
            }
        }

        private static void ActivateExistingInstance()
        {
            AppMessageHandler.SendMessageToExistingInstance();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (args.Contains("--clear-events"))
                {
                    AppMessageHandler.SendClearEventsCommand();
                }
            }
        }
    }
}