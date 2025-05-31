using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Common_Tasks
{
    /// <summary>
    /// Handles inter-process communication between application instances
    /// </summary>
    public static class AppMessageHandler
    {
        // Windows API constants and methods for window management
        private const int WM_SHOWME = 0x8001;  // Custom window message
        private const int WM_CLEAR_EVENTS = 0x8002;  // Custom message for clear events command
        private const int HWND_BROADCAST = 0xffff;
        private const int SW_RESTORE = 9;  // Restore window command
        private const int SW_SHOW = 5;     // Show window command
        
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
        
        // Delegate for the EnumWindows callback
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        
        // Store the main form reference
        private static MainForm _mainForm;
        
        /// <summary>
        /// Registers the message handler for the application
        /// </summary>
        public static void RegisterMessageHandler()
        {
            // Get a reference to the main form
            _mainForm = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
            
            if (_mainForm != null)
            {
                // Override WndProc to handle custom messages
                _mainForm.MessageReceived += HandleMessage;
            }
        }
        
        /// <summary>
        /// Handles incoming window messages
        /// </summary>
        private static void HandleMessage(object sender, Message m)
        {
            // Check for our custom messages
            switch (m.Msg)
            {
                case WM_SHOWME:
                    // Restore and activate the window
                    RestoreWindow();
                    break;
                    
                case WM_CLEAR_EVENTS:
                    // Handle the clear events command
                    if (_mainForm != null)
                    {
                        // Invoke on UI thread if needed
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
        
        /// <summary>
        /// Sends a message to the existing instance to show itself
        /// </summary>
        public static void SendMessageToExistingInstance()
        {
            // Find all windows belonging to our application
            IntPtr hWnd = FindApplicationWindow();
            
            if (hWnd != IntPtr.Zero)
            {
                // Try direct window activation first
                DirectWindowActivation(hWnd);
                
                // Also send the custom message as a backup method
                PostMessage(hWnd, WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
            }
        }
        
        /// <summary>
        /// Sends the clear events command to the existing instance
        /// </summary>
        public static void SendClearEventsCommand()
        {
            // Find all windows belonging to our application
            IntPtr hWnd = FindApplicationWindow();
            
            if (hWnd != IntPtr.Zero)
            {
                // First make sure the window is visible and in the foreground
                DirectWindowActivation(hWnd);
                
                // Then send the clear events command
                // Wait a bit to ensure the window is fully restored
                Thread.Sleep(500);
                PostMessage(hWnd, WM_CLEAR_EVENTS, IntPtr.Zero, IntPtr.Zero);
            }
        }
        
        /// <summary>
        /// Directly activates a window using Windows API calls
        /// </summary>
        private static void DirectWindowActivation(IntPtr hWnd)
        {
            // If the window is minimized, restore it
            if (IsIconic(hWnd))
            {
                ShowWindow(hWnd, SW_RESTORE);
            }
            else
            {
                // Otherwise just make sure it's visible
                ShowWindow(hWnd, SW_SHOW);
            }
            
            // Bring the window to the foreground
            SetForegroundWindow(hWnd);
        }
        
        /// <summary>
        /// Finds the main window of our application
        /// </summary>
        private static IntPtr FindApplicationWindow()
        {
            // First try the simple approach - find by window title
            IntPtr hWnd = FindWindow(null, "Common Tasks");
            
            if (hWnd != IntPtr.Zero)
            {
                return hWnd;
            }
            
            // If that fails, try to find by enumerating all windows
            // and checking their process ID against our current process name
            int currentProcessId = Process.GetCurrentProcess().Id;
            string processName = Process.GetCurrentProcess().ProcessName;
            
            // Get all processes with the same name (but different IDs)
            Process[] processes = Process.GetProcessesByName(processName)
                .Where(p => p.Id != currentProcessId)
                .ToArray();
            
            if (processes.Length > 0)
            {
                // We found at least one other instance of our application
                // Try to find its main window
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
        
        /// <summary>
        /// Restores and activates the main window
        /// </summary>
        private static void RestoreWindow()
        {
            if (_mainForm != null)
            {
                try
                {
                    // Ensure we're on the UI thread
                    if (_mainForm.InvokeRequired)
                    {
                        _mainForm.Invoke(new Action(RestoreWindow));
                        return;
                    }
                    
                    // Show the form if it's hidden
                    _mainForm.Show();
                    
                    // Restore the window if it's minimized
                    if (_mainForm.WindowState == FormWindowState.Minimized)
                    {
                        _mainForm.WindowState = FormWindowState.Normal;
                    }
                    
                    // Make sure it's visible in the taskbar
                    _mainForm.ShowInTaskbar = true;
                    
                    // Hide the tray icon if it's visible
                    if (_mainForm.TaskTrayIcon != null)
                    {
                        _mainForm.TaskTrayIcon.Visible = false;
                    }
                    
                    // Restore the form to its default size if needed
                    _mainForm.RestoreFormSize();
                    _mainForm.RestorePanelSize();
                    
                    // Bring the window to the foreground
                    _mainForm.BringToFront();
                    _mainForm.Activate();
                    _mainForm.Focus();
                    
                    // Force the window to the foreground
                    SetForegroundWindow(_mainForm.Handle);
                    
                    // Use direct Windows API as a backup
                    ShowWindow(_mainForm.Handle, SW_RESTORE);
                    
                    // Add a small delay to ensure UI updates are processed
                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    // Log the error but don't crash
                    MessageBox.Show($"Error restoring window: {ex.Message}");
                }
            }
        }
    }
}
