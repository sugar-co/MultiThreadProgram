using System;
using System.Collections.Generic;
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
        static void AccessDataBase(string name, int seconds)
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
            Console.WriteLine("Starting a long running work...");//2 
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine("Work is done!");// 3
            _workerEvent.Set();
            Console.WriteLine("Waiting for a main thread to complete its work");//4
            _mainEvent.WaitOne();
            Console.WriteLine("Starting second operation...");//7
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine("Work is done!");//9
            _workerEvent.Set();
        }
        static void TraveThroughGates(string threadName, int seconds)
        {
            Console.WriteLine($"{threadName} fails to sleep");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            Console.WriteLine($"{threadName} waits for the gates to open!");
            _mainEvent2.Wait();
            Console.WriteLine($"{threadName} enters the gates");
        }
        static ManualResetEventSlim _mainEvent2 = new ManualResetEventSlim();

        static CountdownEvent _countdown = new CountdownEvent(2);
        static void PerformOperation(string message, int seconds)
        {
            {
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                Console.WriteLine(message);
                _countdown.Signal();
            }
        }
        static Barrier _barrier = new Barrier(2, (b) => Console.WriteLine($"End of phase {b.CurrentPhaseNumber + 1}"));
        static void PlayMusic(string name, string message, int seconds)
        {
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("------------------------------");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                Console.WriteLine($"{name} starts to {message}");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                Console.WriteLine($"{name} finishes to {message}");
                _barrier.SignalAndWait();
            }
        }
        static ReaderWriterLockSlim _rw = new ReaderWriterLockSlim();
        static Dictionary<int, int> _dict_rw = new Dictionary<int, int>();
        static void Read()
        {
            Console.WriteLine("Reading contents of a dictionary");
            while (true)
            {

                try
                {
                    _rw.EnterReadLock();
                    foreach (var item in _dict_rw)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                    }
                }
                finally
                {
                    _rw.ExitReadLock();
                }
            }
        }
        static void Write(string threadName)
        {
            Console.WriteLine("Writing contents of a dictionary");
            while (true)
            {
                try
                {
                    int newKey = new Random().Next(1000);
                    _rw.EnterUpgradeableReadLock();
                    if (!_dict_rw.ContainsKey(newKey))
                    {
                        try
                        {
                            _rw.EnterWriteLock();
                            _dict_rw[newKey] = 1;
                            Console.WriteLine($"New Key {newKey} is added to a dictionary by a {threadName}");
                            Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        }
                        finally
                        {
                            _rw.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    _rw.ExitUpgradeableReadLock();
                }
            }

        }
        static volatile bool _isCompleted = false;
        static void UserModeWait()
        {
            while (!_isCompleted)
            {
                Console.WriteLine(".");
            }
            Console.WriteLine();
            Console.WriteLine("Waiting is complete");
        }
        static void HybridSpinWait()
        {
            var w = new SpinWait();
            while (!_isCompleted)
            {
                w.SpinOnce();
                Console.WriteLine(w.NextSpinWillYield);
            }
            Console.WriteLine("Waiting is complete");
        }
        static void Main(string[] args)
        {
            {
                var t1 = new Thread(UserModeWait);
                var t2 = new Thread(HybridSpinWait);
                Console.WriteLine("Running user mode waiting");
                t1.Start();
                //Thread.Sleep(TimeSpan.FromSeconds(20));
                Thread.Sleep(20);
                _isCompleted = true;
                Thread.Sleep(TimeSpan.FromSeconds(1));
                _isCompleted = false;
                Console.WriteLine("Running hybrid SpinWait construct waiting"); ;
                t2.Start();
                Thread.Sleep(TimeSpan.FromSeconds(50));
                //Thread.Sleep(5);
                _isCompleted = true;
            }
            return;
            {
                new Thread(Read) { IsBackground = true }.Start();
                new Thread(Read) { IsBackground = true }.Start();
                new Thread(Read) { IsBackground = true }.Start();
                new Thread(() => Write("Thread 1")) { IsBackground = true }.Start();
                new Thread(() => Write("Thread 2")) { IsBackground = true }.Start();
                Thread.Sleep(TimeSpan.FromSeconds(30));
            }
            return;
            {
                // 2.8 
                var t1 = new Thread(() => PlayMusic("the guitarist", "paly an amazing solo", 5));
                var t2 = new Thread(() => PlayMusic("the singer", "sing his song", 2));
                var t3 = new Thread(() => PlayMusic("the painter", "drwa the paper", 18));
                t1.Start();
                t2.Start();
                t3.Start();
            }
            return;
            {
                // 2.7 使用 CountDownEvent
                Console.WriteLine("Satarting two operations");
                var t1 = new Thread(() => PerformOperation("Operation 1 is completed", 4));
                var t2 = new Thread(() => PerformOperation("Operation 2 is completed", 5));
                t1.Start();
                t2.Start();
                _countdown.Wait();
                Console.WriteLine("Both operations have been completed");
                _countdown.Dispose();
            }
            return;
            {
                // 2.6 使用manualResetEventSlim
                var t1 = new Thread(() => TraveThroughGates("Thread 1", 5));
                var t2 = new Thread(() => TraveThroughGates("Thread 2", 6));
                var t3 = new Thread(() => TraveThroughGates("Thread 3", 12));
                t1.Start();
                t2.Start();
                t3.Start();
                Thread.Sleep(TimeSpan.FromSeconds(6));
                Console.WriteLine("The gates are now open");
                _mainEvent2.Set();
                Thread.Sleep(TimeSpan.FromSeconds(2));
                _mainEvent2.Reset();
                Console.WriteLine("The gates have been closed");
                Thread.Sleep(TimeSpan.FromSeconds(10));
                Console.WriteLine("The gates are now open for the second time!");
                _mainEvent2.Set();
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.WriteLine("The gates have been closed!");
                _mainEvent2.Reset();
            }
            return;
            {
                // 2.5 使用AutoResetEvent
                var t = new Thread(() => Process(10));
                t.Start();
                Console.WriteLine("Waiting for another thread to complete work");//1
                _workerEvent.WaitOne();
                Console.WriteLine("First operation is completed!");//5
                Console.WriteLine("Performing an operation on a main thread");//6
                Thread.Sleep(TimeSpan.FromSeconds(5));
                _mainEvent.Set();
                Console.WriteLine("Now running the second operation on a second thread");//8
                _workerEvent.WaitOne();
                Console.WriteLine("Second operation is completed!");//10
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
