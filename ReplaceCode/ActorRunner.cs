using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lpubsppop01.ReplaceCode
{
    class ActorRunner
    {
        Actor[] actors;
        ConcurrentQueue<Actor> actorQueue = new ConcurrentQueue<Actor>();
        List<Task> tasks = new List<Task>();

        public ActorRunner(params Actor[] actors)
        {
            this.actors = actors;
        }

        public void Run(int threadCount = 1)
        {
            if (threadCount == 1)
            {
                RunInSerial();
            }
            else if (threadCount > 1)
            {
                RunInParallel(threadCount);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        void RunInSerial()
        {
            foreach (var actor in actors)
            {
                actorQueue.Enqueue(actor);
            }
            while (actorQueue.TryDequeue(out var curr))
            {
                if (curr.MessageLoop())
                {
                    actorQueue.Enqueue(curr);
                }
            }
        }

        void RunInParallel(int threadCount)
        {
            foreach(var actor in actors)
            {
                actorQueue.Enqueue(actor);
            }
            for (int i = 0; i < threadCount; ++i)
            {
                tasks.Add(Task.Run(() =>
                {
                    while (actorQueue.TryDequeue(out var curr))
                    {
                        if (curr.MessageLoop())
                        {
                            actorQueue.Enqueue(curr);
                        }
                    }
                }));
            }
        }

        public void Wait()
        {
            Task.WaitAll(tasks.ToArray());
        }
    }
}
