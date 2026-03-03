using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Question_6._1
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service start run");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Đang chạy lúc: {time}", DateTimeOffset.Now);
                await Task.Delay(30000, stoppingToken);
            }

            _logger.LogInformation("Service Stop");
        }
    }
}