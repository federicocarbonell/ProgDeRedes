using System;
using System.Threading;

namespace Threading
{
    class Program
    {
        static bool Finished;
        static void Main(string[] args)
        {
            //Thread.CurrentThread.Name= "Thread principal";
            //Thread thread = new Thread(() => PrintX(20));
            //thread.Name = "Thread de las y";
            //thread.IsBackground = true;
            //new Thread(() => PrintY(20)).Start();

            for (int i = 0; i < 10; i++)
            {
                int number = i;
                new Thread(() => Print(number)).Start();
            }
        }

        static void Print(int printVariable)
        {
            Console.Write(printVariable);
        }

        static void PrintX(int top)
        {
            Console.WriteLine(Thread.CurrentThread.Name);
            for (int i = 0; i < top; i++)
            {
                Console.Write("X");
            }
            Finished = true;
        }

        static void PrintY(int top)
        {
            Console.WriteLine(Thread.CurrentThread.Name);
            for (int i = 0; i < top; i++)
            {
                Console.Write("Y");
            }
        }

        static void PrintZ(int top)
        {
            Console.WriteLine(Thread.CurrentThread.Name);
            for (int i = 0; i < top; i++)
            {
                Console.Write("Z");
            }
        }
    }
}
