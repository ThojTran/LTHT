using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Tutorial_3_1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Question 1: Stack vs Heap and Object Lifetime");
            Question1_Demo();
        }
        private struct MyStruct
        {
            public int Value;
        }

        private class MyClass
        {
            public int Value;
            public byte[] Payload = new byte[16];
        }
        // - `s_staticObject` là một gốc tĩnh, đối tượng này sẽ tồn tại trong suốt thời gian chạy của ứng dụng.
        static MyClass s_staticObject = new MyClass { Value = 100 }; // Reference stored in static field (in the managed heap's roots)

        private static void Question1_Demo()
        {
            // Kiểu giá trị
            int localInt = 42;
            // - `localInt` is a value type and stored on the stack
            // Nó không tạo ra một lần cấp phát bộ nhớ GC

            // Cấu trúc
            MyStruct localStruct = new MyStruct { Value = 1 };
            // - `localStruct` là một biến cục bộ.

            MyClass localObject = new MyClass { Value = 2 };
            // MyClass được cấp phát trên heap được quản lý
            // Đối tượng đủ điều kiện để được thu gom rác khi không còn tham chiếu 

            Console.WriteLine($"localInt (stack) = {localInt}");
            Console.WriteLine($"localStruct.Value (stack) = {localStruct.Value}");
            Console.WriteLine($"localObject.Value (heap) = {localObject.Value}");
            Console.WriteLine($"s_staticObject.Value (heap, referenced by static root) = {s_staticObject.Value}");

            // Minh hoạ khi được thu gom rác 
            {
                MyClass scoped = new MyClass { Value = 3 };
                Console.WriteLine($"scoped.Value (heap) = {scoped.Value}");
            }
            // `scoped` có thể truy cập trong khối này.
            // Nó ra khỏi phạm vi ở đây và không còn tham chiếu nào trỏ đến nó.

            localObject = null; // Null thể hiện tham chiếu trước đó đủ điều kiện để thu gom rác.

            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("Forced GC. Objects with no reachable references are now collected (if any).");
        }
    }
}