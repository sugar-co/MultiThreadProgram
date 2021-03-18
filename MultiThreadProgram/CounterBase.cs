using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadProgram
{
    public abstract class CounterBase
    {
        public abstract void Increment();
        public abstract void Decrement();
    }

    public class Counter : CounterBase
    {
        public int Count { get; private set; }
        public override void Increment()
        {
            Count++;
        }
        public override void Decrement()
        {
            Count--;
        }
    }

    public class CounterWithLock : CounterBase
    {
        private readonly object _syncRoot = new object();
        public int Count { get; private set; }
        public override void Increment()
        {
            lock (_syncRoot)
            {
                Count++;
            }
        }
        public override void Decrement()
        {
            lock (_syncRoot)
            {
                Count--;
            }
        }
    }
}
