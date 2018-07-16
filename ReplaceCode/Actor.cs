using System;
using System.Collections.Concurrent;

namespace Lpubsppop01.ReplaceCode
{
    abstract class Actor
    {
        bool done;

        public ConcurrentQueue<object> MessageQueue { get; private set; } = new ConcurrentQueue<object>();

        public virtual void Start()
        {
            done = false;
        }

        public bool MessageLoop()
        {
            while (!done)
            {
                if (MessageQueue.IsEmpty)
                {
                    return true;
                }
                if (MessageQueue.TryDequeue(out var message))
                {
                    try
                    {
                        OnMessage(message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            return false;
        }

        protected abstract void OnMessage(object message);

        public virtual void Stop()
        {
            done = true;
        }
    }
}
