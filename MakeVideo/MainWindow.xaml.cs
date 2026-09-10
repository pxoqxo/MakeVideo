using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Pxoqxo.MakeVideo
{
    public partial class MainWindow : Window
    {
        private bool isProcessing = false;
        private Process? process = null;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void BrowseImageBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Select Image...",
                Multiselect = false
            };

            bool? result = dialog.ShowDialog();
            if (result ?? false)
            {
                ImagePathTb.Text = dialog.FileName;
            }
        }
        private void BrowseAudioBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Select Audio...",
                Multiselect = false
            };

            bool? result = dialog.ShowDialog();
            if (result ?? false)
            {
                AudioPathTb.Text = dialog.FileName;
            }
        }
        private void BrowseVideoBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Title = "Save Video..."
            };

            bool? result = dialog.ShowDialog();
            if (result ?? false)
            {
                VideoPathTb.Text = dialog.FileName;
            }
        }
        private async void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isProcessing)
            {
                return;
            }

            isProcessing = true;
            IsEnabled = false;

            ClearMessages();
            AddNormalMessage("Starting video export...");

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "your_video_exporter.exe",
                    Arguments = "--your-export-arguments",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process = new Process
                {
                    StartInfo = startInfo
                };
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (output != string.Empty)
                {
                    AddNormalMessage(output);
                }
                if (error != string.Empty)
                {
                    AddErrorMessage(error);
                }

                if (process.ExitCode == 0)
                {
                    AddSuccessMessage($"Process completed successfully with Exit Code: {process.ExitCode}");
                }
                else
                {
                    AddErrorMessage($"Process completed with errors. Exit Code: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                AddErrorMessage($"Exception: {ex.Message}");
            }
            finally
            {
                if (process != null)
                {
                    process.Dispose();
                    process = null;
                }
                isProcessing = false;
                IsEnabled = true;
            }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!isProcessing)
            {
                return;
            }

            e.Cancel = true;

            MessageBoxResult result = MessageBox.Show(this, "A video export is currently in progress. Closing the application now may result in corrupted files. Are you sure you want to force close?", "Question", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (process != null && !process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    AddErrorMessage($"Exception: {ex.Message}");
                }
                finally
                {
                    e.Cancel = false;
                }
            }
        }

        private void AddNormalMessage(string message)
        {
            AddMessage(message, Brushes.White);
        }
        private void AddErrorMessage(string message)
        {
            AddMessage(message, Brushes.Red);
        }
        private void AddWarningMessage(string message)
        {
            AddMessage(message, Brushes.Blue);
        }
        private void AddSuccessMessage(string message)
        {
            AddMessage(message, Brushes.Lime);
        }
        private void AddMessage(string message, Brush brush)
        {
            if (message == string.Empty)
            {
                return;
            }

            Run run = new Run(message + Environment.NewLine)
            {
                Foreground = brush
            };

            ConsolePara.Inlines.Add(run);
            ConsoleRtb.ScrollToEnd();
        }
        private void ClearMessages()
        {
            ConsolePara.Inlines.Clear();
        }
    }
}
