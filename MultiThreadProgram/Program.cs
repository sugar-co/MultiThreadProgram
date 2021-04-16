using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            {
                // 5.6 处理异步操作中的异常
                Task t = AsynchronousProcessing3();
                t.Wait();
                return;
            }
            {
                // 5.5 对并行执行的异步任务使用await操作符
                Task t = AsynchronousProcessing2();
                t.Wait();
                return;
            }
            {
                // 5.4 对连续的异步任务使用await操作符
                Task t = AsynchronyWithTPL2();
                t.Wait();

                t = AsynchronyWithAwait2();
                t.Wait();
                return;
            }
            {
                // 5.3 在lambda表达式中使用await操作符
                Task t = AsynchronousProcessing();
                t.Wait();
                return;
            }
            {
                // 5.2 使用await操作符获取异步任务结果
                Task t = AsynchronyWithTPL();
                t.Wait();

                t = AsynchronyWithAwait();
                t.Wait();
                return;
            }
            Console.WriteLine("Hello ;World!");
        }

        static async Task AsynchronousProcessing3()
        {
            Console.WriteLine("1. Single exception");
            try
            {
                string result = await GetInfoAsync2("Task 1", 2);
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception details: {ex}");
            }
            Console.WriteLine();
            Console.WriteLine("2. Multiple exceptions");

            Task<string> t1 = GetInfoAsync2("Task 1", 3);
            Task<string> t2 = GetInfoAsync2("Task 2", 2);
            try
            {
                string[] results = await Task.WhenAll(t1, t2);
                Console.WriteLine(results.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception details: {ex}");
            }

            Console.WriteLine();
            Console.WriteLine("3. Multiple exceptions with AggregateException");
            t1 = GetInfoAsync2("Task 1", 3);
            t2 = GetInfoAsync2("Task 2", 2);
            Task<string[]> t3 = Task.WhenAll(t1, t2);
            try
            {
                string[] results = await t3;
                Console.WriteLine(  results.Length);
            }
            catch (Exception ex)
            {
                var ae = t3.Exception.Flatten();
                var exceptions = ae.InnerExceptions;
                Console.WriteLine($"Exception caught: {exceptions.Count}");
                foreach (var e in exceptions)
                {
                    Console.WriteLine($"Exception details: {e}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine("4. Await in catch and finally blocks");
            try
            {
                string result = await GetInfoAsync2("Task 1", 2);
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                Console.WriteLine($"Catch block with await: Exception details: {ex}");
            }finally
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                Console.WriteLine("Finally block");
            }
        }

        static async Task<string> GetInfoAsync2(string name,int seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            throw new Exception($"Boom from {name}");
        }
        
        static async Task AsynchronousProcessing2()
        {
            Task<string> t1 = GetInfoAsync("Task 1", 3);
            Task<string> t2 = GetInfoAsync("Task 2", 5);
            string[] results = await Task.WhenAll(t1, t2);
            foreach (var result in results)
            {
                Console.WriteLine(result);
            }
        }
        static async Task<string> GetInfoAsync(string name,int seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            //await Task.Run(() => Thread.Sleep(TimeSpan.FromSeconds(seconds)));
            return $"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
        }

        static Task AsynchronyWithTPL2()
        {
            var containerTask = new Task(() =>
            {
                Task<string> t = GetInfoAsync("TPL 1");
                t.ContinueWith(task =>
                {
                    Console.WriteLine(t.Result);
                    Task<string> t2 = GetInfoAsync("TPL 2");
                    t2.ContinueWith(innerTask => Console.WriteLine(innerTask.Result), TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.AttachedToParent);
                    t2.ContinueWith(innerTask => Console.WriteLine(innerTask.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.AttachedToParent);

                }, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.AttachedToParent);
                t.ContinueWith(task => Console.WriteLine(t.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.AttachedToParent);
            });
            containerTask.Start();
            return containerTask;
        }
        static async Task AsynchronyWithAwait2()
        {
            try
            {
                string result = await GetInfoAsync("Async 1");
                Console.WriteLine(result);
                result = await GetInfoAsync("Async 2");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static async Task AsynchronousProcessing()
        {
            Func<string, Task<string>> asyncLambda = async name =>
             {
                 await Task.Delay(TimeSpan.FromSeconds(2));
                 return $"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
             };
            string result = await asyncLambda("async lambda");
            Console.WriteLine(result);
        }

        static Task AsynchronyWithTPL()
        {
            Task<string> t = GetInfoAsync("Task 1");
            Task t2 = t.ContinueWith(task => Console.WriteLine(t.Result), TaskContinuationOptions.NotOnFaulted);
            Task t3 = t.ContinueWith(task => Console.WriteLine(t.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted);
            return Task.WhenAny(t2, t3);
        }
        static async Task AsynchronyWithAwait()
        {
            try
            {
                string result = await GetInfoAsync("Task 2");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static async Task<string> GetInfoAsync(string name)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            throw new Exception("Boom!");
            return $"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}";
        }
    }
}
