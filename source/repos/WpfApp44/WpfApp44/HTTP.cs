using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AsyncDemo.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;

        public event Action<string> DataLoaded;
        public event Action<string> StatusUpdated;
        public event Action Completed;

        public HttpService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task LoadDataAsync()
        {
            try
            {
                StatusUpdated?.Invoke("Загрузка данных...");

                string data = await _httpClient.GetStringAsync("https://jsonplaceholder.typicode.com/posts/1");

                DataLoaded?.Invoke(data);
            }
            catch (HttpRequestException ex)
            {
                StatusUpdated?.Invoke($"Ошибка сети: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                StatusUpdated?.Invoke("Таймаут запроса");
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