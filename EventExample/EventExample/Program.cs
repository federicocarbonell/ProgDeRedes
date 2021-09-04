using System;
using System.Threading;

namespace EventExample
{
    class Program
    {
        static void Main(string[] args)
        {
            ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
            Console.WriteLine("Hello World!");
        }
    }
}
