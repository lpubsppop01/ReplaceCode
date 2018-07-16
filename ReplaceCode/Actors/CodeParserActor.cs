using System;
using System.Collections.Concurrent;

namespace Lpubsppop01.ReplaceCode.Actors
{
    class CodeParserActor : Actor, IDisposable
    {
        CodeParser parser;

        public CodeParserActor()
        {
            var paths = AppPathSet.Current;
            if (paths.CSharpParserPath.EndsWith(".dll", ignoreCase: true, culture: null))
            {
                parser = new CodeParser("dotnet", paths.CSharpParserPath);
            }
            else if (paths.CSharpParserPath.EndsWith(".exe", ignoreCase: true, culture: null))
            {
                parser = new CodeParser(paths.CSharpParserPath, "");
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
