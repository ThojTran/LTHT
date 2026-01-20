using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MembershipCardPrinter
{
    class Program
    {
        // Import Windows GDI32 function to install font
        [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
        public static extern int AddFontResource([In][MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            InstallFont();

            Console.WriteLine("\n===== SAMPLE MEMBERSHIP CARDS =====\n");
            PrintCard("Trần Thọ", "CRM001", "VIP Member");
            PrintCard("Trần Lộc", "CRM002", "Premium Member");

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void InstallFont()
        {
            string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LibreBarcode39-Regular.ttf");

            Console.WriteLine("===== FONT INSTALLATION =====");
            Console.WriteLine($"Looking for: {fontPath}\n");

            if (File.Exists(fontPath))
            {
                int result = AddFontResource(fontPath);
                if (result > 0)
                    Console.WriteLine("[SUCCESS] Font installed to memory session\n");
                else
                    Console.WriteLine("[WARNING] Font installation failed\n");
            }
            else
            {
                Console.WriteLine("[ERROR] Font file not found!");
                Console.WriteLine("Please add 'LibreBarcode39-Regular.ttf' to project\n");
            }
        }

        static void PrintCard(string name, string id, string level)
        {
            Console.WriteLine("CRM MEMBERSHIP CARD");
            Console.WriteLine($"Name:  {name,-26}");
            Console.WriteLine($"ID:    {id,-26}");
            Console.WriteLine($"Level: {level,-26}");
            Console.WriteLine($"Barcode: *{id}*{new string(' ', 26 - id.Length - 2)}");
        }
    }
}