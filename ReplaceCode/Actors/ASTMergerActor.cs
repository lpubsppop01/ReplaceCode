using Lpubsppop01.ReplaceCode.Base;
using System.Collections.Concurrent;

namespace Lpubsppop01.ReplaceCode.Actors
{
    class ASTMergerActor : Actor
    {
        int expectedCount = int.MaxValue;
        int count;

        public ConcurrentQueue<object> OutputQueue { get; set; }
        public AST AST;

        protected override void OnMessage(object message)
        {
            if (message is AST ast)
            {
                if (this.AST == null)
                {
                    this.AST = ast;
                }
                else
                {
                    this.AST.Merge(ast);
                }
                if (++count == expectedCount)
                {
                    OutputQueue.Enqueue(this.AST);
                }
            }
            else if (message is int expectedCount)
            {
                this.expectedCount = expectedCount;
                if (count >= expectedCount)
                {
                    OutputQueue.Enqueue(this.AST);
                }
            }
        }
    }
}
