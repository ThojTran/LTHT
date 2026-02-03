using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Question_4._4
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: monitor <watchFolder> <outputFolder> [maxParallelism]");
                return 1;
            }

            var watchFolder = Path.GetFullPath(args[0]);
            var outFolder = Path.GetFullPath(args[1]);
            var maxParallelism = args.Length >= 3 && int.TryParse(args[2], out var p)
                ? p : Environment.ProcessorCount;

            Directory.CreateDirectory(watchFolder);
            Directory.CreateDirectory(outFolder);

            using var monitor = new FileMonitor(watchFolder, outFolder, maxParallelism);
            monitor.Start();

            Console.WriteLine($"Monitoring '{watchFolder}' (Max parallel: {maxParallelism})");
            Console.WriteLine("Press Ctrl+C to exit...");

            await Task.Delay(Timeout.Infinite);
            return 0;
        }
    }

    internal sealed class FileMonitor : IDisposable
    {
        private readonly string _watchFolder;
        private readonly string _outputFolder;
        private readonly FileSystemWatcher _watcher;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentDictionary<string, Timer> _pendingFiles = new();

        public FileMonitor(string watchFolder, string outputFolder, int maxParallelism)
        {
            _watchFolder = watchFolder;
            _outputFolder = outputFolder;
            _semaphore = new SemaphoreSlim(maxParallelism);

            _watcher = new FileSystemWatcher(watchFolder)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*"
            };

            _watcher.Created += OnFileEvent;
            _watcher.Changed += OnFileEvent;
            _watcher.Error += (s, e) => Console.Error.WriteLine($"Watcher error: {e.GetException()}");
        }

        public void Start() => _watcher.EnableRaisingEvents = true;

        private void OnFileEvent(object sender, FileSystemEventArgs e)
        {
            // Bỏ qua temp files
            if (e.Name.EndsWith(".tmp") || e.Name.StartsWith("~")) return;

            // Debounce: chờ 500ms không có event nữa mới xử lý
            _pendingFiles.AddOrUpdate(
                e.FullPath,
                path =>
                {
                    // File mới → tạo timer
                    return new Timer(_ => ProcessFile(path), null, 500, Timeout.Infinite);
                },
                (path, oldTimer) =>
                {
                    // File đã có → reset timer
                    oldTimer.Change(500, Timeout.Infinite);
                    return oldTimer;
                }
            );
        }

        private void ProcessFile(string path)
        {
            // Xóa timer khỏi dictionary
            if (_pendingFiles.TryRemove(path, out var timer))
            {
                timer.Dispose();
            }

            // Xử lý bất đồng bộ (không chặn thread pool)
            _ = Task.Run(async () =>
            {
                await _semaphore.WaitAsync();
                try
                {
                    await CompressFileAsync(path);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error processing '{path}': {ex.Message}");
                }
                finally
                {
                    _semaphore.Release();
                }
            });
        }

        private async Task CompressFileAsync(string path)
        {
            // 1. Kiểm tra file tồn tại
            if (!File.Exists(path)) return;

            // 2. Chờ file ổn định (2 lần check, mỗi lần 200ms)
            long size1 = -1, size2 = 0;
            for (int i = 0; i < 2 && size1 != size2; i++)
            {
                size1 = new FileInfo(path).Length;
                await Task.Delay(200);
                size2 = new FileInfo(path).Length;
            }

            // 3. Đọc file
            byte[] data;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var ms = new MemoryStream())
            {
                await fs.CopyToAsync(ms);
                data = ms.ToArray();
            }

            // 4. Nén và lưu (atomic write)
            var outputPath = Path.Combine(_outputFolder, Path.GetFileName(path) + ".gz");
            var tempPath = outputPath + ".tmp";

            using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var gz = new GZipStream(fs, CompressionLevel.Optimal))
            {
                await gz.WriteAsync(data, 0, data.Length);
            }

            // Di chuyển atomic
            File.Delete(outputPath); // Xóa file cũ nếu có
            File.Move(tempPath, outputPath);

            Console.WriteLine($"✓ Compressed: {Path.GetFileName(path)} → {Path.GetFileName(outputPath)}");
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            _semaphore?.Dispose();

            foreach (var timer in _pendingFiles.Values)
            {
                timer.Dispose();
            }
        }
    }
}