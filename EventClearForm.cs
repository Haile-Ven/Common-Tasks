using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Tasks
{
    public partial class EventClearForm : Form
    {
        int errCount = 0, outCount = 0, totalEvent=0;
        bool canClose = false;

        public EventClearForm()
        {
            InitializeComponent();
            _ = LoadFormAsync();
            ControlBox = false;
        }

        private async Task LoadFormAsync()
        {
            try
            {
                string evtClrPath = "CAEL.bat";
                string prpClrPath = "EVC.bat";
                if (File.Exists(prpClrPath) && File.Exists(evtClrPath))
                {
                    await PrepareEventClearing(prpClrPath);
                    await ClearEventLogs(evtClrPath);
                }
                else
                {
                    MessageBox.Show("Batch file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("Run program as an Administrator!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EventClearForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Check if the closing reason is user closing (e.g., clicking the close button on the taskbar)
            if (e.CloseReason == CloseReason.UserClosing && !canClose)
            {
                // Cancel the closing operation
                e.Cancel = true;
            }
        }

        private void EventClearForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            canClose = true;
        }

        private async Task PrepareEventClearing(string batchFilePath)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/c \"{batchFilePath}\"";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    StringBuilder outputBuilder = new StringBuilder();
                    StringBuilder errorBuilder = new StringBuilder();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            outputBuilder.AppendLine(e.Data);

                            // Marshal UI update to the UI thread
                            if (IsHandleCreated)
                            {
                                _ = BeginInvoke(new Action(() =>
                                {
                                    // Check if the output is a number
                                    if (int.TryParse(e.Data, out int count))
                                    {
                                        // Update totEvtLbl.Text with the formatted number
                                        totEvtLbl.Text = $"( out of {count} )";
                                        totalEvent = count;
                                    }
                                }));
                            }
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            errorBuilder.AppendLine(e.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();

                    // Use Task.Run to wait for the process to exit
                    await Task.Run(() =>
                    {
                        process.WaitForExit();
                    });

                    // Display the final output and error
                    string output = outputBuilder.ToString();
                    string error = errorBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                _ = ex is ObjectDisposedException
                    ? MessageBox.Show("Script execution was interrupted. Try Again.", "Interruption", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    : MessageBox.Show(ex.Message, ex.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ClearEventLogs(string batchFilePath)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/c \"{batchFilePath}\"";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    StringBuilder outputBuilder = new StringBuilder();
                    StringBuilder errorBuilder = new StringBuilder();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            outputBuilder.AppendLine(e.Data);

                            // Marshal UI update to the UI thread
                            if (IsHandleCreated)
                            {
                                BeginInvoke(new Action(() =>
                                {
                                    msgLbl.Text = e.Data;
                                    sucLbl.Text = (outCount).ToString() + " Succeeded.";
                                    if (outCount >= totalEvent) outCount = totalEvent;
                                    else outCount++;
                                    // Update progress bar with each output data received
                                    prgsBar.Value = (outCount * 100) / totalEvent;
                                    prgLbl.Text = $"{prgsBar.Value}%"; // Update prgLbl.Text here
                                }));
                            }
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            errorBuilder.AppendLine(e.Data);

                            // Marshal UI update to the UI thread
                            if (IsHandleCreated)
                            {
                                BeginInvoke(new Action(() =>
                                {
                                    msgLbl.Text = e.Data;
                                    errLbl.Text = (errCount++).ToString() + " Failed.";
                                    // Update progress bar with each error data received
                                    prgLbl.Text = $"{prgsBar.Value}%"; // Update prgLbl.Text here
                                }));
                            }
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();

                    // Use Task.Run to update the progress bar while waiting for the process to exit
                    await Task.Run(() =>
                    {
                        DateTime startTime = DateTime.Now;
                        while (!process.HasExited)
                        {
                            // Sleep for a short duration
                            Thread.Sleep(10);

                            // Marshal UI update to the UI thread
                            if (IsHandleCreated)
                            {
                                BeginInvoke(new Action(() =>
                                {
                                    prgLbl.Text = $"{prgsBar.Value}%"; // Update prgLbl.Text here
                                }));
                            }
                        }
                    });

                    // Ensure the progress bar reaches 100% after completion
                    if (IsHandleCreated)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            prgsBar.Value = 100;
                            prgLbl.Text = "100%"; // Update prgLbl.Text here
                        }));
                    }

                    // Set the flag to allow closing
                    canClose = true;

                    // Display the final output and error
                    string output = outputBuilder.ToString();
                    string error = errorBuilder.ToString();
                    ControlBox = true;
                }
            }
            catch (Exception ex)
            {
                _ = ex is ObjectDisposedException
                    ? MessageBox.Show("Event log Clearing Was Interrupted. Try Again.", "Interruption", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    : MessageBox.Show(ex.Message, ex.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
