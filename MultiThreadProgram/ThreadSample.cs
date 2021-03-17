using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadProgram
{
    class ThreadSample
    {
        private bool _isStopped = false;
        public void Stop()
        {
            _isStopped = true;
        }

        public void CountNumbers()
        {
            long fl = 0;
            double counter = 0;
            while (!_isStopped)
            {
                counter++;
                if (counter > double.MaxValue / 2)
                {
                    counter = 0;
                    fl++;
                }
            }
            Console.WriteLine($"{Thread.CurrentThread.Name} with {Thread.CurrentThread.Priority} has a count = {counter,13:N0}");
        }
    }
}
