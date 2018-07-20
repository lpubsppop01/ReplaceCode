using System;
using System.Collections.Concurrent;

namespace Lpubsppop01.ReplaceCode.Actors
{
    class CodeParserActor : Actor, IDisposable
    {
        CodeParser parser;

        public CodeParserActor()
        {
            var env = AppEnvironment.Current;
            if (env.CSharpParserPath.EndsWith(".dll", ignoreCase: true, culture: null))
            {
                parser = new CodeParser("dotnet", env.CSharpParserPath);
            }
            else if (env.CSharpParserPath.EndsWith(".exe", ignoreCase: true, culture: null))
            {
                parser = new CodeParser(env.CSharpParserPath, "");
            }
        }

        public ConcurrentQueue<object> OutputQueue { get; set; }

        public void Dispose()
        {
            parser.Dispose();
        }

        protected override void OnMessage(object message)
        {
            if (message is string srcFilePath)
            {
                var ast = parser.Parse(srcFilePath);
                Console.WriteLine($"Done: {srcFilePath}");
                OutputQueue.Enqueue(ast);
            }
        }
    }
}
