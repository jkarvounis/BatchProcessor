using System;
using TestShared;

namespace TestAppNet5
{
    class Program
    {
        static void Main(string[] args)
        {
            BatchProcessorSample.Execute("TestAppNet5.exe", "localhost", TimeSpan.FromMinutes(10), args);
        }
    }
}
