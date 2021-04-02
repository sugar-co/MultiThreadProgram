using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadProgram
{
    class Program
    {
        static void TaskMethod(string name)
        {
            Console.WriteLine($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
        }
        static int TaskMethod2(string name)
        {
            Console.WriteLine($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            return 42;
        }
        static int TaskMethod(string name,int seconds)
        {
            Console.WriteLine($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            return 42 * seconds;
        }

        static Task<int> CreateTask(string name)
        {
            return new Task<int>(() => TaskMethod2(name));
        }
        static void Main(string[] args)
        {
            {
                // 4.4 组合任务
                var firstTask = new Task<int>(() => TaskMethod("First Task", 3));
                var secondTask = new Task<int>(() => TaskMethod("Second Task", 2));
                firstTask.ContinueWith(t => Console.WriteLine($"The first answer is {t.Result}. Thread id {Thread.CurrentThread.ManagedThreadId}, is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}"), TaskContinuationOptions.OnlyOnRanToCompletion);
                firstTask.Start();
                secondTask.Start();
                Thread.Sleep(TimeSpan.FromSeconds(4));
                Task continuation = secondTask.ContinueWith(t => Console.WriteLine($"The second answer is {t.Result}. Thread id {Thread.CurrentThread.ManagedThreadId}, is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}"), TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
                continuation.GetAwaiter().OnCompleted(() => Console.WriteLine($"Continuation Task Completed! Thread id {Thread.CurrentThread.ManagedThreadId}, is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}"));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                Console.WriteLine();
                firstTask = new Task<int>(() =>
                {
                    var innerTask = Task.Factory.StartNew(() => TaskMethod("Second Task", 5), TaskCreationOptions.AttachedToParent);
                    innerTask.ContinueWith(t => TaskMethod("Third Task", 2), TaskContinuationOptions.AttachedToParent);
                    return TaskMethod("First Task", 2);
                });
                firstTask.Start();
                while (!firstTask.IsCompleted)
                {
                    Console.WriteLine(firstTask.Status);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
                Console.WriteLine(firstTask.Status);
                Thread.Sleep(TimeSpan.FromSeconds(10));
                return;
            }
            {
                // 4.3 使用任务执行基本的操作
                TaskMethod2(("Main Thread Task"));
                Task<int> task = CreateTask("Task 1");
                task.Start();
                int result = task.Result;
                Console.WriteLine($"Result is: {result}");
                task = CreateTask("Task 2");
                task.RunSynchronously();
                result = task.Result;
                Console.WriteLine($"Result is: {result}");
                task = CreateTask("Task 3");
                Console.WriteLine(task.Status);
                task.Start();
                while (!task.IsCompleted)
                {
                    Console.WriteLine(task.Status);
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
                Console.WriteLine(task.Status);
                result = task.Result;
                Console.WriteLine($"Result is: {result}");
                return;
            }
            {
                // 4.2 创建任务
                var t1 = new Task(() => TaskMethod("Task 1"));
                var t2 = new Task(() => TaskMethod("Task 2"));
                t2.Start();
                t1.Start();
                Task.Run(() => TaskMethod("Task 3"));
                Task.Factory.StartNew(() => TaskMethod("Task 4"));
                Task.Factory.StartNew(() => TaskMethod("Task 5"), TaskCreationOptions.LongRunning);
                Thread.Sleep(TimeSpan.FromSeconds(1));
                return;
            }
        }
    }
}
