using System;
using System.Threading;

namespace MultiThreadProgram
{
    class Program
    {
        static void TestCounter(CounterBase c)
        {
            for (int i = 0; i < 100_000; i++)
            {
                c.Increment();
                c.Decrement();
            }
        }
        static SemaphoreSlim _semaphore = new SemaphoreSlim(4);
        static void AccessDataBase(string name,int seconds)
        {
            Console.WriteLine($"{name} waite to access a database");
            _semaphore.Wait();
            Console.WriteLine($"{name} was granted an access to a database");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine($"{name} is comleted");
            _semaphore.Release();
        }

        private static AutoResetEvent _workerEvent = new AutoResetEvent(false);
        private static AutoResetEvent _mainEvent = new AutoResetEvent(false);
        static void Process(int seconds)
        {
            Console.WriteLine("Starting a long running work...");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine("Work is done!");
            _workerEvent.Set();
            Console.WriteLine("Waiting for a main thread to complete its work");
            _mainEvent.WaitOne();
            Console.WriteLine("Starting second operation...");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine("Work is done!");
            _workerEvent.Set();
        }
        static void Main(string[] args)
        {
            {
                // 2.5 使用AutoResetEvent
                var t = new Thread(() => Process(10));
                t.Start();
                Console.WriteLine("Waiting for another thread to complete work");
                _workerEvent.WaitOne();
                Console.WriteLine("First operation is completed!");
                Console.WriteLine("Performing an operation on a main thread");
                Thread.Sleep(TimeSpan.FromSeconds(5));
                _mainEvent.Set();
                Console.WriteLine("Now running the second operation on a second thread");
                _workerEvent.WaitOne();
                Console.WriteLine("Second operation is completed!");
            }
            return;
            {
                // 2.4 使用SemphoreSlim
                for (int i = 0; i < 6; i++)
                {
                    string threadName = "Thread " + i;
                    int secondsToWait = 2 + 2 * i;
                    var t = new Thread(() => AccessDataBase(threadName, secondsToWait));
                    t.Start();
                }
            }
            return;
            {
                // 2.3 使用Mutex类
                const string MutexName = "CSharpThreadingCookbook";
                using (var m = new Mutex(false, MutexName))
                {
                    if (!m.WaitOne(TimeSpan.FromSeconds(5), false))
                    {
                        Console.WriteLine("Second instance is running");
                    }
                    else
                    {
                        Console.WriteLine("Running!");
                        Console.ReadKey(true);
                        m.ReleaseMutex();
                    }
                }
            }
            return;
            {
                // 2.2 执行基本的原子操作
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
                var c1 = new CounterNoLock();

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

        }
    }
}
