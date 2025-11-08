using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;

namespace ConcurrencyDemo.Services
{
    public class Task2Service
    {
        private readonly Dispatcher _dispatcher;
        private readonly List<string> _items = new List<string>();
        private readonly object _lockObject = new object();
        private Thread _producerThread;
        private Thread _consumerThread;
        private bool _isRunning = false;
        private int _producerCounter = 0;
        private int _consumerCounter = 0;

        public event Action<string> StatusUpdated;
        public event Action<string> ProducerItemAdded;
        public event Action<string> ConsumerItemAdded;
        public event Action Cleared;
        public event Action Started;
        public event Action Stopped;

        public Task2Service(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _items.Clear();
            _producerCounter = 0;
            _consumerCounter = 0;

            UpdateStatus("Запуск производителя и потребителя...");

            _producerThread = new Thread(ProducerWork)
            {
                IsBackground = true
            };

            _consumerThread = new Thread(ConsumerWork)
            {
                IsBackground = true
            };

            _producerThread.Start();
            _consumerThread.Start();

            NotifyStarted();
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;

            lock (_lockObject)
            {
                Monitor.PulseAll(_lockObject);
            }

            _producerThread?.Join(1000);
            _consumerThread?.Join(1000);

            UpdateStatus("Производитель и потребитель остановлены");
            NotifyStopped();
        }

        private void ProducerWork()
        {
            while (_isRunning)
            {
                lock (_lockObject)
                {
                    _producerCounter++;
                    string item = $"Элемент {_producerCounter}";
                    _items.Add(item);

                    _dispatcher.Invoke(() =>
                    {
                        ProducerItemAdded?.Invoke(item);
                    });

                    Monitor.Pulse(_lockObject);
                }

                Thread.Sleep(500);
            }
        }

        private void ConsumerWork()
        {
            while (_isRunning)
            {
                lock (_lockObject)
                {
                    while (_items.Count == 0 && _isRunning)
                    {
                        Monitor.Wait(_lockObject);
                    }

                    if (!_isRunning)
                        break;

                    if (_items.Count > 0)
                    {
                        string item = _items[0];
                        _items.RemoveAt(0);
                        _consumerCounter++;

                        _dispatcher.Invoke(() =>
                        {
                            ConsumerItemAdded?.Invoke(item);
                        });
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private void UpdateStatus(string status)
        {
            _dispatcher.Invoke(() =>
            {
                StatusUpdated?.Invoke(status);
            });
        }

        private void NotifyStarted()
        {
            _dispatcher.Invoke(() =>
            {
                Started?.Invoke();
            });
        }

        private void NotifyStopped()
        {
            _dispatcher.Invoke(() =>
            {
                Stopped?.Invoke();
            });
        }
    }
}