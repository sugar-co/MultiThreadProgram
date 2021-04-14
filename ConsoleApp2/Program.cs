using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {

        delegate string AsynchronousTask(string threadName);
        delegate string IncompatibleAsynchronousTask(out int threadId);
        static void CallBack(IAsyncResult ar)
        {
            Console.WriteLine("Starting a callback...");
            Console.WriteLine($"State passed to a callbak: {ar.AsyncState}");
            Console.WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Console.WriteLine($"Thread pool worker thread id: {Thread.CurrentThread.ManagedThreadId}");
        }
        static string Test(string threadName)
        {
            Console.WriteLine("Starting...");
            Console.WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Thread.CurrentThread.Name = threadName;
            return $"Thread name: {Thread.CurrentThread.Name}";
        }
        static string Test(out int threadId)
        {
            Console.WriteLine("Starting...");
            Console.WriteLine($"Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            threadId = Thread.CurrentThread.ManagedThreadId;
            return $"Thread pool worker thread id was: {threadId}";
        }
        static void Main(string[] args)
        {

            {
                // 4.5 APM模式转换为任务
                int threadId;
                AsynchronousTask d = Test;
                IncompatibleAsynchronousTask e = Test;
                Console.WriteLine("Option 1");
                Task<string> task = Task<string>.Factory.FromAsync(d.BeginInvoke("AsyncTaskThread", CallBack, "a delegate asynchronous call"), d.EndInvoke);
                task.ContinueWith(t => Console.WriteLine($"Callback is finished,now running a continuation! Result: {t.Result}"));
                while (!task.IsCompleted)
                {
                    Console.WriteLine(task.Status);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
                Console.WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(1));
                Console.WriteLine("---------------------------------------");
                Console.WriteLine();

                Console.WriteLine("Option 2");
                task = Task<string>.Factory.FromAsync(d.BeginInvoke, d.EndInvoke, "AsyncTaskThread", "a delegate asynchronous call");
                task.ContinueWith(t => Console.WriteLine($"Task is completed, now running a continuation! Result: {t.Result}"));
                while (!task.IsCompleted)
                {
                    Console.WriteLine(task.Status);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
                Console.WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(1));
                Console.WriteLine("---------------------------------------");
                Console.WriteLine();

                Console.WriteLine("Option 3");
                IAsyncResult ar = e.BeginInvoke(out threadId, CallBack, "a delegate asynchronous call");
                task = Task<string>.Factory.FromAsync(ar, k => e.EndInvoke(out threadId, k));
                task.ContinueWith(t => Console.WriteLine($"Task is completed, now running a continuation! Result: {t.Result}, ThreadId: {threadId}"));
                while (!task.IsCompleted)
                {
                    Console.WriteLine(task.Status);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
                Console.WriteLine(task.Status);
                Thread.Sleep(TimeSpan.FromSeconds(1));
                return;
            }
        }
    }
}
