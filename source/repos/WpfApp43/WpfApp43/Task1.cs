using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;

namespace ConcurrencyDemo.Services
{
    public class Task1Service
    {
        private readonly Dispatcher _dispatcher;
        private readonly List<string> _unsafeList = new List<string>();
        private readonly List<string> _safeList = new List<string>();
        private readonly object _lockObject = new object();
        private int _unsafeCounter = 0;
        private int _safeCounter = 0;

        public event Action<string> StatusUpdated;
        public event Action<string> ItemAdded;
        public event Action ListCleared;
        public event Action TaskCompleted;

        public Task1Service(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void StartRaceCondition()
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    UpdateStatus("Запуск гонки данных...");
                    _unsafeList.Clear();
                    _unsafeCounter = 0;

                    Thread thread1 = new Thread(() => AddUnsafeItems("Поток 1: ", 100));
                    Thread thread2 = new Thread(() => AddUnsafeItems("Поток 2: ", 100));

                    thread1.IsBackground = true;
                    thread2.IsBackground = true;

                    thread1.Start();
                    thread2.Start();

                    thread1.Join();
                    thread2.Join();

                    UpdateStatus($"Гонка данных завершена. Ожидалось: 200, Получено: {_unsafeCounter}");
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Ошибка: {ex.Message}");
                }
                finally
                {
                    NotifyCompleted();
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        private void AddUnsafeItems(string prefix, int count)
        {
            for (int i = 0; i < count; i++)
            {
                _unsafeCounter++;
                string item = $"{prefix}{i}";
                _unsafeList.Add(item);

                _dispatcher.Invoke(() =>
                {
                    ItemAdded?.Invoke(item);
                });

                Thread.Sleep(5);
            }
        }

        public void StartSafeAdd()
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    UpdateStatus("Запуск безопасного добавления...");
                    _safeList.Clear();
                    _safeCounter = 0;

                    Thread thread1 = new Thread(() => AddSafeItems("Поток 1: ", 100));
                    Thread thread2 = new Thread(() => AddSafeItems("Поток 2: ", 100));

                    thread1.IsBackground = true;
                    thread2.IsBackground = true;

                    thread1.Start();
                    thread2.Start();

                    thread1.Join();
                    thread2.Join();

                    UpdateStatus($"Безопасное добавление завершено. Ожидалось: 200, Получено: {_safeCounter}");
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Ошибка: {ex.Message}");
                }
                finally
                {
                    NotifyCompleted();
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        private void AddSafeItems(string prefix, int count)
        {
            for (int i = 0; i < count; i++)
            {
                lock (_lockObject)
                {
                    _safeCounter++;
                    string item = $"{prefix}{i}";
                    _safeList.Add(item);
                }

                _dispatcher.Invoke(() =>
                {
                    ItemAdded?.Invoke($"{prefix}{i}");
                });

                Thread.Sleep(5);
            }
        }

        public void StartMonitorTimeout()
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    UpdateStatus("Проверка Monitor.TryEnter с таймаутом...");

                    bool acquired = Monitor.TryEnter(_lockObject, TimeSpan.FromSeconds(1));

                    if (acquired)
                    {
                        try
                        {
                            UpdateStatus("Блокировка захвачена успешно");

                            Thread thread2 = new Thread(() =>
                            {
                                bool acquired2 = Monitor.TryEnter(_lockObject, TimeSpan.FromMilliseconds(100));

                                if (acquired2)
                                {
                                    try
                                    {
                                        UpdateStatus("Второй поток захватил блокировку");
                                    }
                                    finally
                                    {
                                        Monitor.Exit(_lockObject);
                                    }
                                }
                                else
                                {
                                    UpdateStatus("Второй поток не смог захватить блокировку (таймаут)");
                                }
                            });

                            thread2.IsBackground = true;
                            thread2.Start();

                            Thread.Sleep(500);
                        }
                        finally
                        {
                            Monitor.Exit(_lockObject);
                            UpdateStatus("Блокировка освобождена");
                        }
                    }
                    else
                    {
                        UpdateStatus("Не удалось захватить блокировку");
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Ошибка: {ex.Message}");
                }
                finally
                {
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

        private void NotifyCompleted()
        {
            _dispatcher.Invoke(() =>
            {
                TaskCompleted?.Invoke();
            });
        }
    }
}