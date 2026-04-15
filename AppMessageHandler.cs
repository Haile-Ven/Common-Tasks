using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Common_Tasks
{
    public static class AppMessageHandler
    {
        private const int WM_SHOWME = 0x8001;
        private const int WM_CLEAR_EVENTS = 0x8002;
        private const int HWND_BROADCAST = 0xffff;
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private static MainForm _mainForm;

        public static void RegisterMessageHandler()
        {
            _mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();

            if (_mainForm != null)
            {
                _mainForm.MessageReceived += HandleMessage;
            }
        }
        private static void HandleMessage(object sender, Message m)
        {
            switch (m.Msg)
            {
                case WM_SHOWME:
                    RestoreWindow();
                    break;

                case WM_CLEAR_EVENTS:
                    if (_mainForm != null)
                    {
                        if (_mainForm.InvokeRequired)
                        {
                            _mainForm.Invoke(new Action(() => _mainForm.OpenEventClearForm()));
                        }
                        else
                        {
                            _mainForm.OpenEventClearForm();
                        }
                    }
                    break;
            }
        }

        public static void SendMessageToExistingInstance()
        {
            IntPtr hWnd = FindApplicationWindow();

            if (hWnd != IntPtr.Zero)
            {
                DirectWindowActivation(hWnd);

                PostMessage(hWnd, WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
            }
        }


        public static void SendClearEventsCommand()
        {
            IntPtr hWnd = FindApplicationWindow();

            if (hWnd != IntPtr.Zero)
            {
                DirectWindowActivation(hWnd);

                Thread.Sleep(500);
                PostMessage(hWnd, WM_CLEAR_EVENTS, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private static void DirectWindowActivation(IntPtr hWnd)
        {
            if (IsIconic(hWnd))
            {
                ShowWindow(hWnd, SW_RESTORE);
            }
            else
            {
                ShowWindow(hWnd, SW_SHOW);
            }

            SetForegroundWindow(hWnd);
        }

        private static IntPtr FindApplicationWindow()
        {
            IntPtr hWnd = FindWindow(null, "Common Tasks");

            if (hWnd != IntPtr.Zero)
            {
                return hWnd;
            }

            int currentProcessId = Process.GetCurrentProcess().Id;
            string processName = Process.GetCurrentProcess().ProcessName;

            Process[] processes = Process.GetProcessesByName(processName)
                .Where(p => p.Id != currentProcessId)
                .ToArray();

            if (processes.Length > 0)
            {
                foreach (Process process in processes)
                {
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        return process.MainWindowHandle;
                    }
                }
            }

            return IntPtr.Zero;
        }

        private static void RestoreWindow()
        {
            if (_mainForm != null)
            {
                try
                {
                    if (_mainForm.InvokeRequired)
                    {
                        _mainForm.Invoke(new Action(RestoreWindow));
                        return;
                    }

                    _mainForm.Show();

                    if (_mainForm.WindowState == FormWindowState.Minimized)
                    {
                        _mainForm.WindowState = FormWindowState.Normal;
                    }

                    _mainForm.ShowInTaskbar = true;

                    if (_mainForm.TaskTrayIcon != null)
                    {
                        _mainForm.TaskTrayIcon.Visible = false;
                    }

                    _mainForm.RestoreFormSize();
                    _mainForm.RestorePanelSize();

                    _mainForm.BringToFront();
                    _mainForm.Activate();
                    _mainForm.Focus();

                    SetForegroundWindow(_mainForm.Handle);

                    ShowWindow(_mainForm.Handle, SW_RESTORE);

                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error restoring window: {ex.Message}");
                }
            }
        }
    }
}
