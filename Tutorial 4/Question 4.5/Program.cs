using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Question_4_5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                ShowUsage();
                return;
            }

            try
            {
                var command = args[0].ToLower();
                switch (command)
                {
                    case "encrypt":
                        if (args.Length < 4)
                        {
                            Console.WriteLine("Usage: encrypt <inputFile> <outputFile> <password>");
                            return;
                        }
                        SecureFileStorage.Encrypt(args[1], args[2], args[3]);
                        Console.WriteLine($"✓ Encrypted: {args[1]} → {args[2]}");
                        break;

                    case "decrypt":
                        if (args.Length < 4)
                        {
                            Console.WriteLine("Usage: decrypt <encryptedFile> <outputFile> <password>");
                            return;
                        }
                        SecureFileStorage.Decrypt(args[1], args[2], args[3]);
                        Console.WriteLine($"✓ Decrypted: {args[1]} → {args[2]}");
                        break;

                    default:
                        ShowUsage();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine("Secure File Storage Utility");
            Console.WriteLine("\nUsage:");
            Console.WriteLine("  encrypt <inputFile> <outputFile> <password>");
            Console.WriteLine("  decrypt <encryptedFile> <outputFile> <password>");
            Console.WriteLine("\nExample:");
            Console.WriteLine("  encrypt secret.txt secret.enc myPassword123");
            Console.WriteLine("  decrypt secret.enc decrypted.txt myPassword123");
        }
    }

    /// <summary>
    /// Xử lý mã hóa và nén file một cách an toàn
    /// Process: Read → Encrypt (AES-256) → Compress (GZip) → Save
    /// Reverse: Read → Decompress → Decrypt → Save
    /// </summary>
    public static class SecureFileStorage
    {
        private const int KeySize = 256;        // AES-256
        private const int BlockSize = 128;      // AES block size
        private const int SaltSize = 32;        // Salt cho key derivation
        private const int Iterations = 10000;   // PBKDF2 iterations

        /// <summary>
        /// Mã hóa và nén file
        /// </summary>
        public static void Encrypt(string inputPath, string outputPath, string password)
        {
            // 1. Đọc dữ liệu gốc
            byte[] plainData = File.ReadAllBytes(inputPath);

            // 2. Tạo salt ngẫu nhiên (để derive key từ password)
            byte[] salt = GenerateRandomBytes(SaltSize);

            // 3. Derive key và IV từ password
            using var deriveBytes = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256
            );
            byte[] key = deriveBytes.GetBytes(KeySize / 8);  // 32 bytes
            byte[] iv = deriveBytes.GetBytes(BlockSize / 8); // 16 bytes

            // 4. Mã hóa dữ liệu (AES-256)
            byte[] encryptedData = EncryptAES(plainData, key, iv);

            // 5. Nén dữ liệu đã mã hóa
            byte[] compressedData = Compress(encryptedData);

            // 6. Lưu file: [Salt][Compressed Encrypted Data]
            using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            fs.Write(salt, 0, salt.Length);              // Ghi salt trước
            fs.Write(compressedData, 0, compressedData.Length); // Ghi data sau
        }

        /// <summary>
        /// Giải nén và giải mã file
        /// </summary>
        public static void Decrypt(string encryptedPath, string outputPath, string password)
        {
            // 1. Đọc file
            byte[] fileData = File.ReadAllBytes(encryptedPath);

            // 2. Tách salt và compressed data
            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(fileData, 0, salt, 0, SaltSize);

            byte[] compressedData = new byte[fileData.Length - SaltSize];
            Buffer.BlockCopy(fileData, SaltSize, compressedData, 0, compressedData.Length);

            // 3. Derive key và IV từ password và salt
            using var deriveBytes = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256
            );
            byte[] key = deriveBytes.GetBytes(KeySize / 8);
            byte[] iv = deriveBytes.GetBytes(BlockSize / 8);

            // 4. Giải nén
            byte[] encryptedData = Decompress(compressedData);

            // 5. Giải mã
            byte[] plainData = DecryptAES(encryptedData, key, iv);

            // 6. Lưu file gốc
            File.WriteAllBytes(outputPath, plainData);
        }

        // ==================== ENCRYPTION ====================

        private static byte[] EncryptAES(byte[] data, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        private static byte[] DecryptAES(byte[] data, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(data, 0, data.Length);
        }

        // ==================== COMPRESSION ====================

        private static byte[] Compress(byte[] data)
        {
            using var outputStream = new MemoryStream();
            using (var gzip = new GZipStream(outputStream, CompressionLevel.Optimal))
            {
                gzip.Write(data, 0, data.Length);
            }
            return outputStream.ToArray();
        }

        private static byte[] Decompress(byte[] data)
        {
            using var inputStream = new MemoryStream(data);
            using var gzip = new GZipStream(inputStream, CompressionMode.Decompress);
            using var outputStream = new MemoryStream();
            gzip.CopyTo(outputStream);
            return outputStream.ToArray();
        }

        // ==================== HELPERS ====================

        private static byte[] GenerateRandomBytes(int size)
        {
            byte[] buffer = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buffer);
            return buffer;
        }
    }
}