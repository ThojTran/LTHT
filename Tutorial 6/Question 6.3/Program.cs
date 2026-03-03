using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace Question_6._3
{
    internal class Program
    {
        private static readonly string InputFolder = "Inbox";
        private static readonly string ProcessedFolder = "Processed";
        private static readonly ConcurrentDictionary<string, bool> ProcessingFiles = new();
        private static readonly object DirectoryLock = new object();

        static void Main(string[] args)
        {
            EnsureDirectories();
            CreateTestFiles();

            using (var watcher = new FileSystemWatcher(InputFolder))
            {
                watcher.Filter = "*.json";
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                watcher.Created += OnFileCreated;
                watcher.Error += OnError;
                watcher.EnableRaisingEvents = true;

                Console.WriteLine($"Monitoring {InputFolder} for JSON files...");
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
            }
        }

        private static void EnsureDirectories()
        {
            lock (DirectoryLock)
            {
                if (!Directory.Exists(InputFolder))
                    Directory.CreateDirectory(InputFolder);
                if (!Directory.Exists(ProcessedFolder))
                    Directory.CreateDirectory(ProcessedFolder);
            }
        }

        private static void CreateTestFiles()
        {
            Console.WriteLine("=== Creating test JSON files ===\n");

            // Tạo 3 file JSON test với delay để simulate concurrent arrival
            var testFiles = new[]
            {
                new { Name = "user1.json", Content = "{\"id\":1,\"name\":\"Alice\",\"email\":\"alice@example.com\"}" },
                new { Name = "user2.json", Content = "{\"id\":2,\"name\":\"Bob\",\"email\":\"bob@example.com\"}" },
                new { Name = "user3.json", Content = "{\"id\":3,\"name\":\"Charlie\",\"email\":\"charlie@example.com\"}" }
            };

            Task.Run(async () =>
            {
                for (int i = 0; i < testFiles.Length; i++)
                {
                    string filePath = Path.Combine(InputFolder, testFiles[i].Name);
                    await File.WriteAllTextAsync(filePath, testFiles[i].Content);
                    Console.WriteLine($"[CREATE] {testFiles[i].Name}");

                    // Delay giữa các file để thấy concurrent processing
                    if (i < testFiles.Length - 1)
                        await Task.Delay(300);
                }
            });
        }

        private static void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            // Prevent double processing with thread-safe check
            if (!ProcessingFiles.TryAdd(e.FullPath, true))
            {
                Console.WriteLine($"[SKIP] {Path.GetFileName(e.FullPath)} is already being processed.");
                return;
            }

            // Queue processing to avoid blocking the watcher thread
            Task.Run(() => ProcessFileAsync(e.FullPath));
        }

        private static async Task ProcessFileAsync(string filePath)
        {
            try
            {
                // Wait for file to be fully written
                await WaitForFileAccessAsync(filePath);

                // Read file contents
                string jsonContent = await File.ReadAllTextAsync(filePath);
                Console.WriteLine($"[READ] {Path.GetFileName(filePath)}");

                // Simulate processing
                ValidateJson(jsonContent);
                await SimulateProcessingAsync();

                // Move to Processed folder
                string fileName = Path.GetFileName(filePath);
                string destinationPath = Path.Combine(ProcessedFolder, fileName);

                lock (DirectoryLock)
                {
                    if (File.Exists(destinationPath))
                        File.Delete(destinationPath);
                    File.Move(filePath, destinationPath);
                }

                Console.WriteLine($"[PROCESSED] {fileName} -> {ProcessedFolder}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Processing {Path.GetFileName(filePath)}: {ex.Message}");
            }
            finally
            {
                // Remove from processing set
                ProcessingFiles.TryRemove(filePath, out _);
            }
        }

        private static async Task WaitForFileAccessAsync(string filePath, int maxAttempts = 10)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        return; // File is accessible
                    }
                }
                catch (IOException)
                {
                    if (i < maxAttempts - 1)
                        await Task.Delay(100);
                    else
                        throw;
                }
            }
        }

        private static void ValidateJson(string content)
        {
            try
            {
                using (var doc = JsonDocument.Parse(content))
                {
                    // JSON is valid
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid JSON format: {ex.Message}", ex);
            }
        }

        private static async Task SimulateProcessingAsync()
        {
            // Simulate work (e.g., database insert, API call)
            await Task.Delay(500);
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            Exception ex = e.GetException();
            if (ex is InternalBufferOverflowException)
            {
                Console.WriteLine("[ERROR] Buffer overflow. Increase InternalBufferSize or reduce file event frequency.");
            }
            else if (ex != null)
            {
                Console.WriteLine($"[ERROR] FileSystemWatcher error: {ex.Message}");
            }
        }
    }
}