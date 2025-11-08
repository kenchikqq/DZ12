using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AsyncDemo.Services
{
    public class CpuBoundService
    {
        public event Action<int> ProgressUpdated;
        public event Action<string> ResultUpdated;
        public event Action<string> StatusUpdated;
        public event Action Completed;

        public async Task CalculateSumAsync()
        {
            try
            {
                StatusUpdated?.Invoke("Вычисление начато...");

                long sum = await Task.Run(() =>
                {
                    long result = 0;
                    int totalIterations = 10000000;

                    for (int i = 0; i <= totalIterations; i++)
                    {
                        result += i;

                        if (i % 100000 == 0)
                        {
                            int progress = (int)((double)i / totalIterations * 100);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ProgressUpdated?.Invoke(progress);
                            });
                        }
                    }

                    return result;
                });

                ProgressUpdated?.Invoke(100);
                ResultUpdated?.Invoke(sum.ToString("N0"));
            }
            catch (Exception ex)
            {
                StatusUpdated?.Invoke($"Ошибка: {ex.Message}");
            }
            finally
            {
                Completed?.Invoke();
            }
        }
    }
}