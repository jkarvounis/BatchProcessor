using TestShared;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            BatchProcessorSample.Execute("TestApp.exe", "localhost", null, args);
        }
    }
}
