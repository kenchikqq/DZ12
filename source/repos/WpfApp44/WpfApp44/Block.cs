using System;
using System.Threading;
using System.Windows.Controls;

namespace AsyncDemo.Services
{
    public class BlockingService
    {
        public void StartBlockingOperation(TextBlock statusText)
        {
            statusText.Text = "Операция выполняется...";

            Thread.Sleep(5000);

            statusText.Text = "Операция завершена";
        }
    }
}