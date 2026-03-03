using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Question_6._2
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private string _inputFolder = string.Empty;
        private string _processedFolder = string.Empty;
        private int _intervalSeconds = 30; // default

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        // Đọc config từ Registry
        private bool LoadConfiguration()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\TradingService");

                if (key == null)
                {
                    _logger.LogError("Registry key không tồn tại: HKLM\\SOFTWARE\\TradingService");
                    return false;
                }

                _inputFolder = key.GetValue("InputFolder")?.ToString() ?? string.Empty;
                _processedFolder = key.GetValue("ProcessedFolder")?.ToString() ?? string.Empty;
                var intervalObj = key.GetValue("IntervalSeconds");

                // Validate
                if (string.IsNullOrEmpty(_inputFolder))
                {
                    _logger.LogError("Config không hợp lệ: InputFolder bị trống");
                    return false;
                }

                if (string.IsNullOrEmpty(_processedFolder))
                {
                    _logger.LogError("Config không hợp lệ: ProcessedFolder bị trống");
                    return false;
                }

                if (intervalObj == null || !int.TryParse(intervalObj.ToString(), out _intervalSeconds))
                {
                    _logger.LogWarning("IntervalSeconds không hợp lệ, dùng mặc định: 30 giây");
                    _intervalSeconds = 30;
                }

                _logger.LogInformation("Config loaded: InputFolder={input}, ProcessedFolder={processed}, Interval={interval}s",
                    _inputFolder, _processedFolder, _intervalSeconds);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đọc Registry");
                return false;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service start run");

            if (!LoadConfiguration())
            {
                _logger.LogError("Không load được config, service dừng lại.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Đang chạy lúc: {time}", DateTimeOffset.Now);
                await Task.Delay(_intervalSeconds * 1000, stoppingToken);
            }

            _logger.LogInformation("Service Stop");
        }
    }
}