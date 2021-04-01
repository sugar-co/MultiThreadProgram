using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace MultiThreadProgram
{
    class Program
    {
        private delegate string RunOnThreadPool(out int threadId);
        private static void Callback(IAsyncResult ar)
        {
            Console.WriteLine("Starting a callback...");
            Console.WriteLine($"State passwd to a callback: {ar.AsyncState}");
            Console.WriteLine($"Is thread pool thread:{Thread.CurrentThread.IsThreadPoolThread}");
            Console.WriteLine($"Thread pool worker thread id:{Thread.CurrentThread.ManagedThreadId}");
        }

        private static string Test(out int threadId)
        {
            Console.WriteLine("Starting...");
            Console.WriteLine($"Is thread pool thread:{Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            threadId = Thread.CurrentThread.ManagedThreadId;
            return $"Thread pool worker thread id was:{threadId}";
        }

        private static void AsyncOperation(object state)
        {
            Console.WriteLine($"Operation state: {state ?? "{null}"}");
            Console.WriteLine($"Worker thread id: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        static void UseThreads(int numberOfOperations)
        {
            using (var countdown = new CountdownEvent(numberOfOperations))
            {
                Console.WriteLine("Scheduling work by creating threads");
                for (int i = 0; i < numberOfOperations; i++)
                {
                    var thread = new Thread(() =>
                    {
                        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countdown.Signal();
                    });
                    thread.Start();
                }
                countdown.Wait();
                Console.WriteLine();
            }
        }
        static void UseThreadPool(int numberOfOperations)
        {
            using (var countdown = new CountdownEvent(numberOfOperations))
            {
                {
                    Console.WriteLine("Starting work on a threadpool");
                    for (int i = 0; i < numberOfOperations; i++)
                    {
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId},");
                            Thread.Sleep(TimeSpan.FromSeconds(0.1));
                            countdown.Signal();
                        });
                    }
                }
                countdown.Wait();
                Console.WriteLine();
            }
        }


        static void AsyncOperation1(CancellationToken token)
        {
            Console.WriteLine("Starting the first task");
            for (int i = 0; i < 5; i++)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("The first task has been canceled.");
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            Console.WriteLine("The first task has completed successfully");
        }
        static void AsyncOperation2(CancellationToken token)
        {
            try
            {
                Console.WriteLine("Starting the second task");
                for (int i = 0; i < 5; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
                Console.WriteLine("The second task has completed successfully");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("The second task has been canceled.");
            }
        }
        static void AsyncOperation3(CancellationToken token)
        {
            bool cancellatioonFlag = false;
            token.Register(() => cancellatioonFlag = true);
            Console.WriteLine("Starting the third task");
            for (int i = 0; i < 5; i++)
            {
                if (cancellatioonFlag)
                {
                    Console.WriteLine("The third task has been canceled.");
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            Console.WriteLine("The third task has completed successfully");
        }
        static void RunOperations(TimeSpan workerOperationTimeout)
        {
            using var evt = new ManualResetEvent(false);
            using var cts = new CancellationTokenSource();
            Console.WriteLine("Registering timeout operation...");
            var worker = ThreadPool.RegisterWaitForSingleObject(evt, (state, isTimedOUt) => WorkerOperationWait(cts, isTimedOUt), null, workerOperationTimeout, true);
            Console.WriteLine("Starting long running operation...");
            ThreadPool.QueueUserWorkItem(_ => WorkerOperation(cts.Token, evt));
            Thread.Sleep(workerOperationTimeout.Add(TimeSpan.FromSeconds(2)));
            worker.Unregister(evt);
        }
        static void WorkerOperation(CancellationToken token, ManualResetEvent evt)
        {
            for (int i = 0; i < 6; i++)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            evt.Set();
        }
        static void WorkerOperationWait(CancellationTokenSource cts, bool isTimedOut)
        {
            if (isTimedOut)
            {
                cts.Cancel();
                Console.WriteLine("Worker operation timed out and was canceled.");
            }
            else
            {
                Console.WriteLine("Worker operation succeded.");
            }
        }

        static Timer _timer;
        static void TimerOperation(DateTime start)
        {
            TimeSpan elapsed = DateTime.Now - start;
            Console.WriteLine($"{elapsed.Seconds} seconds from {start}. Timer thread pool thread id: {Thread.CurrentThread.ManagedThreadId}");
        }


        static void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine($"DoWork thread pool thread id:{Thread.CurrentThread.ManagedThreadId}");
            var bw = (BackgroundWorker)sender;
            for (int i = 0; i <= 100; i++)
            {
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                bw.ReportProgress(i);
                //if (i % 10 == 0)
                //{
                //    bw.ReportProgress(i);
                //}
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }
            e.Result = 42;
        }
        static void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine($"{e.ProgressPercentage}% completed. Progress thread pool thread id: {Thread.CurrentThread.ManagedThreadId}");
        }
        static void Worker_Completed(object sender,RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine($"Completed thread pool thread id:{Thread.CurrentThread.ManagedThreadId}");
            if(e.Error!=null)
            {
                Console.WriteLine($"Exception {e.Error.Message} has occured.");
            }else if(e.Cancelled)
            {
                Console.WriteLine($"Operation has been canceled.");
            }
            else
            {
                Console.WriteLine($"The answer is: {e.Result}");
            }
        }

        static void Main(string[] args)
        {
            {
                // 3.8 使用BackgroundWorker组件
                var bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.WorkerSupportsCancellation = true;
                bw.DoWork += Worker_DoWork;
                bw.ProgressChanged += Worker_ProgressChanged;
                bw.RunWorkerCompleted += Worker_Completed;
                bw.RunWorkerAsync();
                Console.WriteLine("Press c to cancel work");
                do
                {
                    if(Console.ReadKey(true).KeyChar == 'C')
                    {
                        bw.CancelAsync();
                    }
                } while (bw.IsBusy);
                return;
            }
            {
                // 3.7 使用计时器
                Console.WriteLine("Press 'Enter' to stop the timer...");
                DateTime start = DateTime.Now;
                _timer = new Timer(_ => TimerOperation(start), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
                try
                {
                    Thread.Sleep(TimeSpan.FromSeconds(6));
                    _timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4));
                    Console.ReadKey(true);
                }
                finally
                {
                    _timer.Dispose();
                }
                return;
            }
            {
                // 3.6 在线程池中使用等待事件处理器及超时
                RunOperations(TimeSpan.FromSeconds(5));
                RunOperations(TimeSpan.FromSeconds(7));
                return;
            }
            {
                // 3.5 实现一个取消选项
                using (var cts = new CancellationTokenSource())
                {
                    CancellationToken token = cts.Token;
                    ThreadPool.QueueUserWorkItem(_ => AsyncOperation1(token));
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    cts.Cancel();
                }
                Console.ReadKey();
                using (var cts = new CancellationTokenSource())
                {
                    CancellationToken token = cts.Token;
                    ThreadPool.QueueUserWorkItem(_ => AsyncOperation2(token));
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    cts.Cancel();
                }
                Console.ReadKey();
                using (var cts = new CancellationTokenSource())
                {
                    CancellationToken token = cts.Token;
                    ThreadPool.QueueUserWorkItem(_ => AsyncOperation3(token));
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    cts.Cancel();
                }
                Thread.Sleep(TimeSpan.FromSeconds(2));
                return;
            }
            {
                // 3.4 线程池与并行度
                const int numberOfOperations = 500;
                var sw = new Stopwatch();
                sw.Start();
                UseThreads(numberOfOperations);
                sw.Stop();
                Console.WriteLine($"Execution time using threads:{sw.ElapsedMilliseconds}ms");
                sw.Reset();
                Console.ReadKey(true);
                sw.Start();
                UseThreadPool(numberOfOperations);
                sw.Stop();
                Console.WriteLine($"Execution time using thread pool:{sw.ElapsedMilliseconds}ms");
                return;
            }
            {
                // 3.3 向线程池中放入异步操作
                const int x = 1;
                const int y = 2;
                const string lambdaState = "lambda state 2";
                ThreadPool.QueueUserWorkItem(AsyncOperation);
                Thread.Sleep(TimeSpan.FromSeconds(10));
                ThreadPool.QueueUserWorkItem(AsyncOperation, "async state");
                Thread.Sleep(TimeSpan.FromSeconds(10));
                ThreadPool.QueueUserWorkItem(state =>
                {
                    Console.WriteLine($"Operation state: {state}");
                    Console.WriteLine($"Worker thread id: {Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }, "lambda state");
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Console.WriteLine($"Operation state: {x + y}, {lambdaState}");
                    Console.WriteLine($"Worker thread id: {Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }, "lambda state");
                Thread.Sleep(TimeSpan.FromSeconds(2));
                return;
            }
            {
                // 3.2 在线程池中调用委托
                int threadId = 0;
                RunOnThreadPool poolDelegate = Test;
                var t = new Thread(() => Test(out threadId));
                t.Start();
                t.Join();
                Console.WriteLine($"Thread id: {threadId}");
                IAsyncResult r = poolDelegate.BeginInvoke(out threadId, Callback, "a delegate asynchronous call");
                r.AsyncWaitHandle.WaitOne();
                string result = poolDelegate.EndInvoke(out threadId, r);
                Console.WriteLine($"Thread pool worker thread id:{threadId}");
                Console.WriteLine(result);
                Thread.Sleep(TimeSpan.FromSeconds(2));
                return;
            }
            Console.WriteLine("Hello World!");
        }
    }
}
