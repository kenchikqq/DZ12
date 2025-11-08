using System.Windows;
using ThreadDemo.Services;

namespace ThreadDemo
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
            _task1Service.ProgressUpdated += (value) =>
            {
                progressBar.Value = value;
            };

            _task1Service.ProgressStatusUpdated += (status) =>
            {
                progressStatusText.Text = status;
            };

            _task1Service.PriorityStatusUpdated += (status) =>
            {
                priorityStatusText.Text = status;
            };

            _task1Service.ErrorOccurred += (error) =>
            {
                errorText.Text = error;
            };

            _task1Service.TaskCompleted += () =>
            {
                startProgressButton.IsEnabled = true;
                startPriorityButton.IsEnabled = true;
                tryUpdateUiButton.IsEnabled = true;
            };
        }

        private void SetupTask2Events()
        {
            _task2Service.LowestValueUpdated += (value) =>
            {
                lowestValueText.Text = value.ToString();
            };

            _task2Service.NormalValueUpdated += (value) =>
            {
                normalValueText.Text = value.ToString();
            };

            _task2Service.HighestValueUpdated += (value) =>
            {
                highestValueText.Text = value.ToString();
            };

            _task2Service.StatusUpdated += (status) =>
            {
                comparisonStatusText.Text = status;
            };

            _task2Service.TaskCompleted += () =>
            {
                startPriorityComparisonButton.IsEnabled = true;
            };
        }

        private void StartProgressButton_Click(object sender, RoutedEventArgs e)
        {
            startProgressButton.IsEnabled = false;
            _task1Service.StartProgressThread();
        }

        private void StartPriorityButton_Click(object sender, RoutedEventArgs e)
        {
            startPriorityButton.IsEnabled = false;
            _task1Service.StartPriorityThread();
        }

        private void TryUpdateUiButton_Click(object sender, RoutedEventArgs e)
        {
            tryUpdateUiButton.IsEnabled = false;
            _task1Service.TryUpdateUiWithoutDispatcher();
        }

        private void StartPriorityComparisonButton_Click(object sender, RoutedEventArgs e)
        {
            startPriorityComparisonButton.IsEnabled = false;
            _task2Service.StartPriorityComparison();
        }
    }
}