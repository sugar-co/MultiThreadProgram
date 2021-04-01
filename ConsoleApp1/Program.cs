using System;
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

        static void Main(string[] args)
        {
            {
                // 3.2 在线程池中调用委托
                int threadId = 0;
                RunOnThreadPool poolDelegate = Test;
                var t = new Thread(() => Test(out threadId));
                t.Start();
                t.Join();
                //Console.ReadKey(true);
                Console.WriteLine($"Thread id: {threadId}");
                IAsyncResult r = poolDelegate.BeginInvoke(out threadId, Callback, "a delegate asynchronous call");
                //r.AsyncWaitHandle.WaitOne();
                string result = poolDelegate.EndInvoke(out threadId, r);
                Console.WriteLine($"Thread pool worker thread id:{threadId}");
                Console.WriteLine(result);
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.ReadKey(true);
                return;
            }
        }
    }
}