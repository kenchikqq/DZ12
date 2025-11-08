using System;
using System.Threading;
using System.Windows.Threading;

namespace ThreadDemo.Services
{
    public class Task2Service
    {
        private readonly Dispatcher _dispatcher;

        public event Action<int> LowestValueUpdated;
        public event Action<int> NormalValueUpdated;
        public event Action<int> HighestValueUpdated;
        public event Action<string> StatusUpdated;
        public event Action TaskCompleted;

        public Task2Service(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void StartPriorityComparison()
        {
            Thread mainThread = new Thread(() =>
            {
                try
                {
                    UpdateStatus("Запуск потоков с разными приоритетами...");

                    int lowestValue = 0;
                    int normalValue = 0;
                    int highestValue = 0;

                    Thread lowestThread = new Thread(() =>
                    {
                        for (int i = 1; i <= 100; i++)
                        {
                            Thread.Sleep(100);
                            lowestValue = i;
                            _dispatcher.Invoke(() =>
                            {
                                LowestValueUpdated?.Invoke(i);
                            });
                        }
                    });

                    Thread normalThread = new Thread(() =>
                    {
                        for (int i = 1; i <= 100; i++)
                        {
                            Thread.Sleep(100);
                            normalValue = i;
                            _dispatcher.Invoke(() =>
                            {
                                NormalValueUpdated?.Invoke(i);
                            });
                        }
                    });

                    Thread highestThread = new Thread(() =>
                    {
                        for (int i = 1; i <= 100; i++)
                        {
                            Thread.Sleep(100);
                            highestValue = i;
                            _dispatcher.Invoke(() =>
                            {
                                HighestValueUpdated?.Invoke(i);
                            });
                        }
                    });

                    lowestThread.Priority = ThreadPriority.Lowest;
                    normalThread.Priority = ThreadPriority.Normal;
                    highestThread.Priority = ThreadPriority.Highest;

                    lowestThread.IsBackground = true;
                    normalThread.IsBackground = true;
                    highestThread.IsBackground = true;

                    UpdateStatus("Потоки запущены. Lowest, Normal, Highest");

                    lowestThread.Start();
                    normalThread.Start();
                    highestThread.Start();

                    lowestThread.Join();
                    normalThread.Join();
                    highestThread.Join();

                    UpdateStatus($"Все потоки завершены. Lowest: {lowestValue}, Normal: {normalValue}, Highest: {highestValue}");
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Ошибка: {ex.Message}");
                }
                finally
                {
                    NotifyTaskCompleted();
                }
            });

            mainThread.IsBackground = true;
            mainThread.Start();
        }

        private void UpdateStatus(string status)
        {
            _dispatcher.Invoke(() =>
            {
                StatusUpdated?.Invoke(status);
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