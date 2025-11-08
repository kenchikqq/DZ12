using System.Windows;
using AsyncDemo.Services;

namespace AsyncDemo
{
    public partial class MainWindow : Window
    {
        private readonly CpuBoundService _cpuBoundService;
        private readonly HttpService _httpService;
        private readonly BlockingService _blockingService;

        public MainWindow()
        {
            InitializeComponent();

            _cpuBoundService = new CpuBoundService();
            _httpService = new HttpService();
            _blockingService = new BlockingService();

            SetupCpuBoundEvents();
            SetupHttpEvents();
        }

        private void SetupCpuBoundEvents()
        {
            _cpuBoundService.ProgressUpdated += (value) =>
            {
                sumProgressBar.Value = value;
            };

            _cpuBoundService.ResultUpdated += (result) =>
            {
                sumResultText.Text = $"Результат: {result}";
            };

            _cpuBoundService.StatusUpdated += (status) =>
            {
                sumResultText.Text = status;
            };

            _cpuBoundService.Completed += () =>
            {
                calculateSumButton.IsEnabled = true;
            };
        }

        private void SetupHttpEvents()
        {
            _httpService.DataLoaded += (data) =>
            {
                httpResultText.Text = $"Данные: {data}";
            };

            _httpService.StatusUpdated += (status) =>
            {
                httpResultText.Text = status;
            };

            _httpService.Completed += () =>
            {
                loadDataButton.IsEnabled = true;
            };
        }

        private async void CalculateSumButton_Click(object sender, RoutedEventArgs e)
        {
            calculateSumButton.IsEnabled = false;
            sumProgressBar.Value = 0;
            await _cpuBoundService.CalculateSumAsync();
        }

        private async void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            loadDataButton.IsEnabled = false;
            await _httpService.LoadDataAsync();
        }

        private void BlockingOperationButton_Click(object sender, RoutedEventArgs e)
        {
            blockingOperationButton.IsEnabled = false;
            _blockingService.StartBlockingOperation(blockingStatusText);
            blockingOperationButton.IsEnabled = true;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            sumProgressBar.Value = 0;
            sumResultText.Text = "Результат: ожидание";
            httpResultText.Text = "Данные: ожидание";
            blockingStatusText.Text = "Статус: ожидание";
        }
    }
}