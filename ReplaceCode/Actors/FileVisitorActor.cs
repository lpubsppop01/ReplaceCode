using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;

namespace Lpubsppop01.ReplaceCode.Actors
{
    class FileVisitorActor : Actor
    {
        Regex targetRegex;
        Regex ignoreRegex;
        int count;

        public FileVisitorActor(Regex targetRegex, Regex ignoreRegex)
        {
            this.targetRegex = targetRegex;
            this.ignoreRegex = ignoreRegex;
            count = 0;
        }

        public ConcurrentQueue<object> FilePathQueue { get; set; }
        public ConcurrentQueue<object> FileCountQueue { get; set; }
        public long? ASTTimestamp { get; set; }

        protected override void OnMessage(object message)
        {
            if (message is string path)
            {
                OnPath(path);
            }
            else if (message == null)
            {
                FileCountQueue.Enqueue(count);
            }
        }

        void OnPath(string path)
        {
            if (File.Exists(path))
            {
                OnFilePath(path);
            }
            else if (Directory.Exists(path))
            {
                OnDirectoryPath(path);
            }
            else
            {
                throw new FileNotFoundException(path);
            }
        }

        void OnFilePath(string path)
        {
            if (ignoreRegex != null && ignoreRegex.IsMatch(path)) return;
            if (ASTTimestamp.HasValue)
            {
                var fileTimestamp = new DateTimeOffset(File.GetLastWriteTime(path)).ToUnixTimeSeconds();
                if (fileTimestamp < ASTTimestamp.Value) return;
            }
            if (targetRegex == null || targetRegex.IsMatch(Path.GetFileName(path)))
            {
                FilePathQueue.Enqueue(path);
                ++count;
            }
        }

        void OnDirectoryPath(string path)
        {
            if (ignoreRegex != null && ignoreRegex.IsMatch(path)) return;
            foreach (var childFilePath in Directory.EnumerateFiles(path))
            {
                OnFilePath(childFilePath);
            }
            foreach (var childDirPath in Directory.EnumerateDirectories(path))
            {
                OnDirectoryPath(childDirPath);
            }
        }
    }
}
