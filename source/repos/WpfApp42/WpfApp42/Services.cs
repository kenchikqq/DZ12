using System;
using System.Threading;
using System.Windows.Threading;

namespace ThreadDemo.Services
{
    public class Task1Service
    {
        private readonly Dispatcher _dispatcher;

        public event Action<int> ProgressUpdated;
        public event Action<string> ProgressStatusUpdated;
        public event Action<string> PriorityStatusUpdated;
        public event Action<string> ErrorOccurred;
        public event Action TaskCompleted;

        public Task1Service(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void StartProgressThread()
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    UpdateProgressStatus("Поток запущен");

                    for (int i = 0; i <= 100; i++)
                    {
                        Thread.Sleep(50);

                        _dispatcher.Invoke(() =>
                        {
                            ProgressUpdated?.Invoke(i);
                        });
                    }

                    UpdateProgressStatus("Поток завершен");
                }
                catch (Exception ex)
                {
                    UpdateError($"Ошибка в потоке прогресса: {ex.Message}");
                }
                finally
                {
                    NotifyTaskCompleted();
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        public void StartPriorityThread()
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    UpdatePriorityStatus($"Поток запущен. Приоритет: {Thread.CurrentThread.Priority}");

                    Thread.Sleep(500);

                    Thread.CurrentThread.Priority = ThreadPriority.Highest;
                    UpdatePriorityStatus($"Приоритет изменен на: {Thread.CurrentThread.Priority}");

                    Thread.Sleep(500);

                    Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                    UpdatePriorityStatus($"Приоритет изменен на: {Thread.CurrentThread.Priority}");

                    Thread.Sleep(500);

                    Thread.CurrentThread.Priority = ThreadPriority.Normal;
                    UpdatePriorityStatus($"Приоритет возвращен на: {Thread.CurrentThread.Priority}");

                    UpdatePriorityStatus("Поток завершен");
                }
                catch (Exception ex)
                {
                    UpdateError($"Ошибка в потоке приоритета: {ex.Message}");
                }
                finally
                {
                    NotifyTaskCompleted();
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        public void TryUpdateUiWithoutDispatcher()
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    UpdateError("Попытка обновить UI без Dispatcher...");
                    Thread.Sleep(500);

                    try
                    {
                        _dispatcher.Invoke(() =>
                        {
                            ErrorOccurred?.Invoke("Попытка прямого обновления UI из фонового потока...");
                        });

                        Thread.Sleep(500);

                        UpdateError("Пробуем без диспатчер - ошибка");
                        Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        UpdateError($"ОШИБКА: {ex.Message}\nТип: {ex.GetType().Name}");
                    }
                }
                catch (Exception ex)
                {
                    UpdateError($"Критическая ошибка: {ex.Message}");
                }
                finally
                {
                    NotifyTaskCompleted();
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        private void UpdateProgressStatus(string status)
        {
            _dispatcher.Invoke(() =>
            {
                ProgressStatusUpdated?.Invoke(status);
            });
        }

        private void UpdatePriorityStatus(string status)
        {
            _dispatcher.Invoke(() =>
            {
                PriorityStatusUpdated?.Invoke(status);
            });
        }

        private void UpdateError(string error)
        {
            _dispatcher.Invoke(() =>
            {
                ErrorOccurred?.Invoke(error);
            });
        }

        private void NotifyTaskCompleted()
        {
            _dispatcher.Invoke(() =>
            {
                TaskCompleted?.Invoke();
            });
        }
    }
}