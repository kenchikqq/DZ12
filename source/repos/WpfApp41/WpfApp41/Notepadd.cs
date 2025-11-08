using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace ProcessThreadDemo.Services
{
    public class NotepadService
    {
        private readonly Dispatcher _dispatcher;

        public event Action<string> StatusUpdated;
        public event Action<string> ResultUpdated;
        public event Action Completed;

        public NotepadService(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void StartNotepadAndCloseAfter3Seconds()
        {
            Thread thread = new Thread(() =>
            {
                Process notepadProcess = null;
                try
                {
                    UpdateStatus("Запуск Notepad...");

                    notepadProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "notepad.exe",
                            UseShellExecute = true
                        }
                    };

                    notepadProcess.Start();

                    UpdateStatus($"Notepad запущен (ID процесса: {notepadProcess.Id})");
                    UpdateResult($"Notepad запущен в процессе с ID: {notepadProcess.Id}\n");

                    Thread.Sleep(3000);

                    if (!notepadProcess.HasExited)
                    {
                        notepadProcess.Kill();
                        notepadProcess.WaitForExit();

                        UpdateStatus("Notepad завершен через 3 секунды");
                        UpdateResult("Notepad автоматически завершен через 3 секунды.\n");
                    }
                    else
                    {
                        UpdateStatus("Notepad завершился самостоятельно");
                        UpdateResult("Notepad завершился самостоятельно.\n");
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Ошибка: {ex.Message}");
                    UpdateResult($"Ошибка при работе с Notepad: {ex.Message}\n");
                }
                finally
                {
                    notepadProcess?.Dispose();
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