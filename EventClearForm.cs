using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SelfSampleProRAD_DB.UserControls;

namespace Common_Tasks
{
    public partial class EventClearForm : Form
    {
        int errCount = 0, outCount = 0, totalEvent = 0;
        bool canClose = false;
        private ToastNotification toastNotification;

        public EventClearForm()
        {
            InitializeComponent();
            
            // Initialize and attach toast notification
            toastNotification = new ToastNotification();
            toastNotification.AttachToForm(this);
            _ = LoadFormAsync();
        }

        private async Task LoadFormAsync()
        {
            try
            {
                await PrepareEventClearing(BatContenet.EventCountBatchContent);
                await ClearEventLogs(BatContenet.EventClearBatchContent);
                await Task.Delay(5000);
                Close();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                toastNotification.Show("Run program as an Administrator!!", "WARNING", false);
            }
        }

        private void EventClearForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !canClose)
            {
                e.Cancel = true;
            }
        }

        private void EventClearForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            canClose = true;
        }

        private async Task PrepareEventClearing(string batchContent)
        {
            await ExecuteBatchContentAsync(batchContent, outputData =>
            {
                if (int.TryParse(outputData, out int count))
                {
                    if (IsHandleCreated)
                    {
                        _ = BeginInvoke(new Action(() =>
                        {
                            totEvtLbl.Text = $"( out of {count} )";
                            totalEvent = count;
                        }));
                    }
                }
            });
        }

        private async Task ClearEventLogs(string batchContent)
        {
            await ExecuteBatchContentAsync(batchContent, outputData =>
            {
                if (IsHandleCreated)
                {
                    BeginInvoke(new Action(() =>
                    {
                        msgLbl.Text = outputData;
                        if(outCount!= totalEvent) sucLbl.Text = $"{++outCount} Succeeded.";
                        prgsBar.Value = outCount * 100 / totalEvent;
                        prgLbl.Text = $"{prgsBar.Value}%";
                    }));
                }
            },
            errorData =>
            {
                if (IsHandleCreated)
                {
                    BeginInvoke(new Action(() =>
                    {
                        msgLbl.Text = errorData;
                        errLbl.Text = $"{++errCount} Failed.";
                        prgLbl.Text = $"{prgsBar.Value}%";
                    }));
                }
            });

            if (IsHandleCreated)
            {
                BeginInvoke(new Action(() =>
                {
                    prgsBar.Value = 100;
                    prgLbl.Text = "100%";
                }));
            }

            canClose = true;
        }

        private async Task ExecuteBatchContentAsync(string batchContent, Action<string> outputHandler, Action<string> errorHandler = null)
        {
            try
            {
                // Write the batch content to a temporary file
                string tempBatchFile = Path.Combine(Path.GetTempPath(), "tempBatch.bat");
                await File.WriteAllTextAsync(tempBatchFile, batchContent);

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/c \"{tempBatchFile}\"";
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
                            outputHandler?.Invoke(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            if (outCount != totalEvent)
                            {
                                errorBuilder.AppendLine(e.Data);
                                errorHandler?.Invoke(e.Data);
                            }
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await process.WaitForExitAsync();

                    string output = outputBuilder.ToString();
                    string error = errorBuilder.ToString();
                }

                // Clean up the temporary file
                if (File.Exists(tempBatchFile))
                {
                    File.Delete(tempBatchFile);
                }
            }
            catch (Exception ex)
            {
                toastNotification.Show(ex.Message, "ERROR", false);
            }
        }
    }
}
