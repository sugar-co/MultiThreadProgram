using System;
using System.Diagnostics;
using System.Threading;

namespace MultiThreadProgram
{
    class Program
    {
        static void RunThreads()
        {
            var sample = new ThreadSample();
            var threadOne = new Thread(sample.CountNumbers);
            threadOne.Name = "ThreadOne";
            var threadTwo = new Thread(sample.CountNumbers);
            threadTwo.Name = "ThreadTwo";

            threadOne.Priority = ThreadPriority.Highest;
            threadTwo.Priority = ThreadPriority.Lowest;
            threadOne.Start();
            threadTwo.Start();

            Thread.Sleep(TimeSpan.FromSeconds(2));
            sample.Stop();
        }
        static void Count(object iterations)
        {
            CountNumbers((int)iterations);
        }
        static void CountNumbers(int iterfations)
        {
            for (int i = 0; i < iterfations; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Console.WriteLine($"{Thread.CurrentThread.Name} prints {i}");
            }
        }
        static void PrintNumber(int number)
        {
            Console.WriteLine(number);
        }

        static void Main(string[] args)
        {
            {
                
            }
            return;
            {
                // 1.8 前台线程和后台线程
                var sampleForeground = new ThreadSample2(10);
                var sampleBackground = new ThreadSample2(20);

                var threadOne = new Thread(sampleForeground.CountNumbers);
                threadOne.Name = "Foreground";
                var threadTwo = new Thread(sampleBackground.CountNumbers);
                threadTwo.Name = "Background";
                threadTwo.IsBackground = true;

                threadOne.Start();
                threadTwo.Start();
            }
            return;
            {
                Console.WriteLine(Thread.GetCurrentProcessorId());
                Console.WriteLine(Process.GetCurrentProcess().ProcessorAffinity);
                // 1.7线程优先级
                Console.WriteLine($"Current thread priority: {Thread.CurrentThread.Priority}");
                Console.WriteLine("Running on all cores available");
                RunThreads();
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.WriteLine("Running on a single core");
                Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
                RunThreads();
            }
            return;
            {
                // 1.6 检测线程状态
                Console.WriteLine("Starting Program...");
                Thread t = new Thread(PrintNumbersWithStatus);
                Thread t2 = new Thread(DoNothing);
                Console.WriteLine($"t.ThreadState:{t.ThreadState}");
                t2.Start();
                t.Start();
                for (int i = 0; i < 30; i++)
                {
                    Console.WriteLine($"t.ThreadState:{t.ThreadState}");
                }
                Thread.Sleep(TimeSpan.FromSeconds(36));
                Console.WriteLine($"t.ThreadState:{t.ThreadState}");
                Console.WriteLine($"t2.ThreadState:{t2.ThreadState}");
            }
            return;
            // 1.5 终止线程
            {
                Console.WriteLine("Starting program...");
                Thread t = new Thread(PrintNumbersWithDelay);
                t.Start();
                Thread.Sleep(TimeSpan.FromSeconds(6));
                {
                    //从 .NET 5.0 开始，以下 API 标记为已过时
                    //使用 CancellationToken 中止对工作单元的处理，而不是调用 Thread.Abort。
                    //t.Abort();
                }
                Console.WriteLine("A thread has been aborted");
                Thread t1 = new Thread(PrintNumbers);
                t1.Start();
                PrintNumbers();
            }
            return;
            // 1.4 等待线程
            {
                Console.WriteLine("Starting...");
                Thread t = new Thread(PrintNumbersWithDelay);
                t.Start();
                t.Join();
                Console.WriteLine("Thread completed");
            }
            return;
            {
                // 1.2 创建线程
                Thread t = new Thread(PrintNumbersWithDelay);
                t.Start();
                PrintNumbers();
            }
        }
        //95118


        static void DoNothing()
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        static void PrintNumbersWithStatus()
        {
            Console.WriteLine("Starting...");
            Console.WriteLine($"Thread.CurrentThread.ThreadState:{Thread.CurrentThread.ThreadState}");
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.WriteLine(i);
            }
        }

        static void PrintNumbers()
        {
            for (int i = 0; i < 10; i++)
            {

                Console.WriteLine($"{i}--{Thread.CurrentThread.ManagedThreadId}");
            }
        }

        static void PrintNumbersWithDelay()
        {
            for (int i = 0; i < 10; i++)
            {

                Console.WriteLine($"{i}--{Thread.CurrentThread.ManagedThreadId}");
                // 1.3 暂停线程
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
