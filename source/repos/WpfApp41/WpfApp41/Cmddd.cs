using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace ProcessThreadDemo.Services
{
    public class CmdService
    {
        private readonly Dispatcher _dispatcher;

        public event Action<string> StatusUpdated;
        public event Action<string> ResultUpdated;
        public event Action Completed;

        public CmdService(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void ExecuteDirCommand()
        {
            Thread thread = new Thread(() =>
            {
                Process cmdProcess = null;
                try
                {
                    UpdateStatus("Выполнение команды dir...");
                    UpdateResult("Выполнение команды dir...\n");

                    cmdProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = "/c",
                            UseShellExecute = false,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true,
                            WorkingDirectory = Environment.CurrentDirectory
                        }
                    };

                    cmdProcess.Start();

                    UpdateStatus("Отправка команды dir...");

                    using (StreamWriter input = cmdProcess.StandardInput)
                    {
                        input.WriteLine("dir");
                        input.WriteLine("exit");
                    }

                    string output = cmdProcess.StandardOutput.ReadToEnd();
                    string error = cmdProcess.StandardError.ReadToEnd();

                    cmdProcess.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        UpdateStatus("Ошибка выполнения команды");
                        UpdateResult($"Ошибка выполнения команды:\n{error}\n");
                    }
                    else
                    {
                        UpdateStatus("Команда выполнена успешно");
                        UpdateResult($"Результат выполнения команды dir в {Environment.CurrentDirectory}:\n\n{output}");
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Ошибка: {ex.Message}");
                    UpdateResult($"Ошибка при выполнении команды: {ex.Message}\n");
                }
                finally
                {
                    cmdProcess?.Dispose();
                    NotifyCompleted();
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        private void UpdateStatus(string status)
        {
            _dispatcher.Invoke(() =>
            {
                StatusUpdated?.Invoke(status);
            });
        }

        private void UpdateResult(string result)
        {
            _dispatcher.Invoke(() =>
            {
                ResultUpdated?.Invoke(result);
            });
        }

        private void NotifyCompleted()
        {
            _dispatcher.Invoke(() =>
            {
                Completed?.Invoke();
            });
        }
    }
}