using System;
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
            using (Mutex mutex = new Mutex(true, MutexName, out bool createdNew))
            {
                if (createdNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    MainForm mainForm = new MainForm();
                    Application.Run(mainForm);
                }
                else
                {
                    MessageBox.Show("Another instance of the application is already running.", "Application Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}