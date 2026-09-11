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
            ExportBtn.IsEnabled = false;

            ClearMessages();

            string imagePath = ImagePathTb.Text;
            string audioPath = AudioPathTb.Text;
            string videoPath = VideoPathTb.Text;

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = @"C:\Users\pxoqxo\Desktop\Demo\ffmpeg.exe",
                ArgumentList =
                {
                    "-y",
                    "-loop", "1",
                    "-i", imagePath,
                    "-i", audioPath,
                    "-c:v", "libx264",
                    "-tune", "stillimage",
                    "-pix_fmt", "yuv420p",
                    "-c:a", "aac",
                    "-shortest",
                    videoPath
                },
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            try
            {
                AddMessage("Starting video export...");
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    AddMessage($"Export completed successfully (Exit Code 0)");
                }
                else
                {
                    AddMessage($"Export failed with Exit Code {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                AddMessage($"Exception: {ex.Message}");
            }
            finally
            {
                process.Dispose();
                process = null;
                isProcessing = false;
                ExportBtn.IsEnabled = true;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!isProcessing)
            {
                return;
            }

            e.Cancel = true;

            MessageBoxResult result = MessageBox.Show(this,
                                      "A video export is currently in progress. Closing the application now may result in corrupted files. Are you sure you want to force close?",
                                      "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

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
                    AddMessage($"Exception: {ex.Message}");
                }
                finally
                {
                    e.Cancel = false;
                }
            }
        }
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            AddMessage(e.Data ?? string.Empty);
        }
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            AddMessage(e.Data ?? string.Empty);
        }

        private void AddMessage(string message)
        {
            if (message == string.Empty)
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                Run run = new Run(message + Environment.NewLine)
                {
                    Foreground = Brushes.White
                };
                ConsolePara.Inlines.Add(run);

                if (!ConsoleRtb.IsMouseOver)
                {
                    ConsoleRtb.ScrollToEnd();
                }
            });
        }
        private void ClearMessages()
        {
            ConsolePara.Inlines.Clear();
        }
    }
}
