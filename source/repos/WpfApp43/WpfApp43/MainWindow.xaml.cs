using System.Windows;
using ConcurrencyDemo.Services;

namespace ConcurrencyDemo
{
    public partial class MainWindow : Window
    {
        private readonly Task1Service _task1Service;
        private readonly Task2Service _task2Service;

        public MainWindow()
        {
            InitializeComponent();

            _task1Service = new Task1Service(Dispatcher);
            _task2Service = new Task2Service(Dispatcher);

            SetupTask1Events();
            SetupTask2Events();
        }

        private void SetupTask1Events()
        {
            _task1Service.StatusUpdated += (status) =>
            {
                task1StatusText.Text = status;
            };

            _task1Service.ItemAdded += (item) =>
            {
                task1ListBox.Items.Add(item);
            };

            _task1Service.ListCleared += () =>
            {
                task1ListBox.Items.Clear();
            };

            _task1Service.TaskCompleted += () =>
            {
                raceConditionButton.IsEnabled = true;
                safeAddButton.IsEnabled = true;
                monitorTimeoutButton.IsEnabled = true;
            };
        }

        private void SetupTask2Events()
        {
            _task2Service.StatusUpdated += (status) =>
            {
                task2StatusText.Text = status;
            };

            _task2Service.ProducerItemAdded += (item) =>
            {
                producerListBox.Items.Add(item);
                producerCountText.Text = producerListBox.Items.Count.ToString();
            };

            _task2Service.ConsumerItemAdded += (item) =>
            {
                consumerListBox.Items.Add(item);
                consumerCountText.Text = consumerListBox.Items.Count.ToString();
            };

            _task2Service.Cleared += () =>
            {
                producerListBox.Items.Clear();
                consumerListBox.Items.Clear();
                producerCountText.Text = "0";
                consumerCountText.Text = "0";
            };

            _task2Service.Started += () =>
            {
                startProducerConsumerButton.IsEnabled = false;
                stopProducerConsumerButton.IsEnabled = true;
            };

            _task2Service.Stopped += () =>
            {
                startProducerConsumerButton.IsEnabled = true;
                stopProducerConsumerButton.IsEnabled = false;
            };
        }

        private void RaceConditionButton_Click(object sender, RoutedEventArgs e)
        {
            raceConditionButton.IsEnabled = false;
            task1ListBox.Items.Clear();
            _task1Service.StartRaceCondition();
        }

        private void SafeAddButton_Click(object sender, RoutedEventArgs e)
        {
            safeAddButton.IsEnabled = false;
            task1ListBox.Items.Clear();
            _task1Service.StartSafeAdd();
        }

        private void MonitorTimeoutButton_Click(object sender, RoutedEventArgs e)
        {
            monitorTimeoutButton.IsEnabled = false;
            task1ListBox.Items.Clear();
            _task1Service.StartMonitorTimeout();
        }

        private void StartProducerConsumerButton_Click(object sender, RoutedEventArgs e)
        {
            producerListBox.Items.Clear();
            consumerListBox.Items.Clear();
            _task2Service.Start();
        }

        private void StopProducerConsumerButton_Click(object sender, RoutedEventArgs e)
        {
            _task2Service.Stop();
        }
    }
}