using Lpubsppop01.ReplaceCode.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lpubsppop01.ReplaceCode.Actors
{
    class GetASTActor : Actor, IDisposable
    {
        IList<string> srcPaths;
        long timestamp;

        List<Actor> childActors;
        FileVisitorActor fileVisitor;
        List<CodeParserActor> parsers;
        LoadBalancerActor parserBalancer;
        ASTMergerActor astMerger;

        public GetASTActor(IList<string> srcPaths, int parserThreadCount)
        {
            this.srcPaths = srcPaths;

            if (parserThreadCount == 1)
            {
                astMerger = new ASTMergerActor { OutputQueue = MessageQueue };
                parsers = new List<CodeParserActor>
                {
                    new CodeParserActor { OutputQueue = astMerger.MessageQueue }
                };
                fileVisitor = new FileVisitorActor(new Regex(@".*\.cs$"), new Regex(@"(\.git|\.vs|bin|obj)$"))
                {
                    FilePathQueue = parsers[0].MessageQueue,
                    FileCountQueue = astMerger.MessageQueue
                };
                childActors = new Actor[] { astMerger, fileVisitor }.Concat(parsers).ToList();
            }
            else
            {
                astMerger = new ASTMergerActor { OutputQueue = MessageQueue };
                parsers = new List<CodeParserActor>
                {
                    new CodeParserActor { OutputQueue = astMerger.MessageQueue },
                    new CodeParserActor { OutputQueue = astMerger.MessageQueue },
                    new CodeParserActor { OutputQueue = astMerger.MessageQueue },
                    new CodeParserActor { OutputQueue = astMerger.MessageQueue }
                };
                parserBalancer = new LoadBalancerActor(parsers.ToArray());
                fileVisitor = new FileVisitorActor(new Regex(@".*\.cs$"), new Regex(@"(\.git|\.vs|bin|obj)$"))
                {
                    FilePathQueue = parserBalancer.MessageQueue,
                    FileCountQueue = astMerger.MessageQueue
                };
                childActors = new Actor[] { astMerger, parserBalancer, fileVisitor }.Concat(parsers).ToList();
            }
        }

        public AST Result { get; private set; }

        public IEnumerable<Actor> Actors => new[] { this }.Concat(childActors);

        public void Dispose()
        {
            parsers.ForEach(p => p.Dispose());
        }

        public override void Start()
        {
            base.Start();
            timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            childActors.ForEach(actor => actor.Start());
            foreach (var srcPath in srcPaths)
            {
                fileVisitor.MessageQueue.Enqueue(srcPath);
            }
            fileVisitor.MessageQueue.Enqueue(null);
        }

        protected override void OnMessage(object message)
        {
            if (message is AST ast)
            {
                ast.TargetPaths = srcPaths.ToArray();
                ast.Timestamp = timestamp;
                Result = ast;
                Stop();
            }
        }

        public override void Stop()
        {
            base.Stop();
            childActors.ForEach(actor => actor.Stop());
        }
    }
}
