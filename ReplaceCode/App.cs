using Lpubsppop01.ReplaceCode.Base;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Lpubsppop01.ReplaceCode.Tests")]

namespace Lpubsppop01.ReplaceCode
{
    class App
    {
        public static App Current { get; private set; } = new App();

        App()
        {
        }

        public AST GetAST(string[] paths, int threadCount, AppOptions options)
        {
            if (paths == null) throw new ArgumentNullException();
            if (paths.Length == 0) throw new ArgumentException();
            if (threadCount < 1) throw new ArgumentException();

            var sw = Stopwatch.StartNew();

            string cacheKey = null;
            if (options.HasFlag(AppOptions.UseCache))
            {
                AppCache.LoadCurrent();
                cacheKey = string.Join(';', paths);
                if (AppCache.Current.SourcePathToASTPath.TryGetValue(cacheKey, out var astPath))
                {
                    var ast = AST.Load(astPath);
                    AppCache.Current.LastASTPath = astPath;
                    AppCache.SaveCurrent();
                    Console.WriteLine($"Loaded in {sw.Elapsed} / {ast.Nodes.Count} nodes / {ast.SourceMap.FilePaths.Count} files");
                    return ast;
                }
            }

            var rootActor = new Actors.GetASTActor(paths, 1);
            rootActor.Start();
            var runner = new ActorRunner(rootActor.Actors.ToArray());
            runner.Run(threadCount);
            runner.Wait();

            if (options.HasFlag(AppOptions.UseCache))
            {
                var astName = Guid.NewGuid().ToString() + ".json";
                var astPath = Path.Combine(AppEnvironment.Current.MyLocalAppDataPath, astName);
                rootActor.Result.Save(astPath);
                AppCache.Current.SourcePathToASTPath[cacheKey] = astPath;
                AppCache.Current.LastASTPath = astPath;
                AppCache.SaveCurrent();
            }

            Console.WriteLine($"Built in {sw.Elapsed} / {rootActor.Result.Nodes.Count} nodes / {rootActor.Result.SourceMap.FilePaths.Count} files");

            if (options.HasFlag(AppOptions.AutoUpdate))
            {
                rootActor.Result.Edited += (sender, e) => UpdateAST(sender as AST, threadCount, options);
            }
            return rootActor.Result;
        }

        public AST UpdateAST(AST ast, int threadCount, AppOptions options)
        {
            if (ast == null) throw new ArgumentNullException();
            if (threadCount < 1) throw new ArgumentException();

            var sw = Stopwatch.StartNew();

            var rootActor = new Actors.UpdateASTActor(ast, 1);
            rootActor.Start();
            var runner = new ActorRunner(rootActor.Actors.ToArray());
            runner.Run(threadCount);
            runner.Wait();

            if (options.HasFlag(AppOptions.UseCache))
            {
                var astName = Guid.NewGuid().ToString() + ".json";
                var astPath = Path.Combine(AppEnvironment.Current.MyLocalAppDataPath, astName);
                rootActor.Result.Save(astPath);
                //ASTCache.Current.SourcePathToASTPath[cacheKey] = astPath;
                AppCache.Current.LastASTPath = astPath;
                AppCache.SaveCurrent();
            }

            Console.WriteLine($"Updated in {sw.Elapsed} / {rootActor.Result.Nodes.Count} nodes / {rootActor.Result.SourceMap.FilePaths.Count} files");

            return rootActor.Result;
        }

        public void SaveAST(AST ast, string path)
        {
            if (ast == null) throw new ArgumentNullException();
            if (path == null) throw new ArgumentNullException();

            ast.Save(path);
        }

        public AST LoadAST(string path)
        {
            if (path == null) throw new ArgumentNullException();

            return AST.Load(path);
        }

        public void ClearASTCache()
        {
            if (!Directory.Exists(AppEnvironment.Current.MyLocalAppDataPath)) return;
            foreach (var path in Directory.EnumerateFiles(AppEnvironment.Current.MyLocalAppDataPath))
            {
                File.Delete(path);
            }
        }

        public void AddWorkspace(string name)
        {
            if (AppSettings.Current.Workspaces.Any(w => w.Name == name)) throw new ArgumentException();
            var newWorkspace = new Workspace { Name = name, Paths = new string[0] };
            AppSettings.Current.Workspaces = AppSettings.Current.Workspaces.Append(newWorkspace).ToArray();
        }

        public void RemoveWorkspace(string name)
        {
            var iMatched = Array.FindIndex(AppSettings.Current.Workspaces, (w) => w.Name == name);
            if (iMatched == -1) throw new ArgumentException();
            var matched = AppSettings.Current.Workspaces[iMatched];
            AppSettings.Current.Workspaces = AppSettings.Current.Workspaces.Where(w => w != matched).ToArray();
            if (AppSettings.Current.CurrentWorkspaceIndex > iMatched)
            {
                --AppSettings.Current.CurrentWorkspaceIndex;
            }
        }

        public Workspace[] GetWorkspaces()
        {
            return AppSettings.Current.Workspaces;
        }
    }
}
