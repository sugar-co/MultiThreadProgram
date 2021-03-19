using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

    public class CounterNoLock : CounterBase
    {
        public int Count { get => _count; }
        private int _count;
        public override void Increment()
        {
            Interlocked.Increment(ref _count);
        }
        public override void Decrement()
        {
            Interlocked.Decrement(ref _count);
        }
    }
}
