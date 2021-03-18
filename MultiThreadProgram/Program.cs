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

        static void TestCounter(CounterBase c)
        {
            for (int i = 0; i < 100_000; i++)
            {
                c.Increment();
                c.Decrement();
            }
        }

        static void LockTooMuch(object lock1, object lock2)
        {
            lock (lock1)
            {
                Thread.Sleep(1000);
                lock (lock2)
                {
                    ;
                }
            }
        }
        static void BadFaultyThread()
        {
            Console.WriteLine("Starting a faulty thread...");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            throw new Exception("Boom!");
        }
        static void FaultyThread()
        {
            try
            {
                Console.WriteLine("Starting a faulty thread...");
                Thread.Sleep(TimeSpan.FromSeconds(1));
                throw new Exception("Boom");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception handled: {ex.Message}");
            }
        }
        static void Main(string[] args)
        {
            {
                // 1.12 处理异常
                var t = new Thread(FaultyThread);
                t.Start();
                t.Join();

                try
                {
                    t = new Thread(BadFaultyThread);
                    t.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("We won't get here!");
                }
            }
            return;
            {
                // 1.11 使用Minitor 类锁定资源
                object lock1 = new object();
                object lock2 = new object();

                new Thread(() => LockTooMuch(lock1, lock2)).Start();

                lock (lock2)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Monitor.TryEnter allows not to get stuck,returning false after a specified timeout is elapsed");
                    if (Monitor.TryEnter(lock1, TimeSpan.FromSeconds(5)))
                    {
                        Console.WriteLine("Acquired a protected resource succesfully");
                    }
                    else
                    {
                        Console.WriteLine("Timeout acquiring a resource");
                    }
                }

                new Thread(() => LockTooMuch(lock1, lock2)).Start();

                Console.WriteLine("--------------------------");
                lock (lock2)
                {
                    Console.WriteLine("This will be a deadlock!");
                    Thread.Sleep(1000);
                    lock (lock1)
                    {
                        Console.WriteLine("Acquiring a protected resource succesfully");
                    }
                }
            }
            return;
            {
                // 1.10 使用c#中的lock关键字
                Console.WriteLine("Incorrect counter");
                var c = new Counter();
                var t1 = new Thread(() => TestCounter(c));
                var t2 = new Thread(() => TestCounter(c));
                var t3 = new Thread(() => TestCounter(c));
                t1.Start();
                t2.Start();
                t3.Start();
                t1.Join();
                t2.Join();
                t3.Join();
                Console.WriteLine($"Total count: {c.Count}");
                Console.WriteLine("--------------------------");

                Console.WriteLine("Correct counter");
                var c1 = new CounterWithLock();

                t1 = new Thread(() => TestCounter(c1));
                t2 = new Thread(() => TestCounter(c1));
                t3 = new Thread(() => TestCounter(c1));
                t1.Start();
                t2.Start();
                t3.Start();
                t1.Join();
                t2.Join();
                t3.Join();
                Console.WriteLine($"Total count: {c1.Count}");
                Console.WriteLine("--------------------------");
            }
            return;
            {
                //1.9 向线程传递参数
                var sample = new ThreadSample2(10);
                var threadOne = new Thread(sample.CountNumbers);
                threadOne.Name = "ThreadOne";
                threadOne.Start();
                threadOne.Join();
                Console.WriteLine("----------------------------");
                var threadTwo = new Thread(Count);
                threadTwo.Name = "ThreadTwo";
                threadTwo.Start(8);
                threadTwo.Join();
                Console.WriteLine("----------------------------");
                var threadThree = new Thread(() => CountNumbers(12));
                threadThree.Name = "ThreadThree";
                threadThree.Start();
                threadThree.Join();
                Console.WriteLine("----------------------------");

                int i = 10;
                var threadFour = new Thread(() => PrintNumber(i));
                i = 20;
                var threadFive = new Thread(() => PrintNumber(i));
                threadFour.Start();
                threadFive.Start();
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
