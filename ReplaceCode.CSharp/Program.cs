using Lpubsppop01.ReplaceCode.Base;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;

namespace Lpubsppop01.ReplaceCode.CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.Name = "ReplaceCode.CSharp";
            app.Description = "A component of Replace Code.";
            app.HelpOption("-h|--help");
            var pathArg = app.Argument("path", "target file path (stdin if not passed)", multipleValues: true);
            app.OnExecute(() =>
            {
                var builder = new ASTBuilder();
                if (pathArg.Values.Any())
                {
                    foreach (var path in pathArg.Values)
                    {
                        HandlePath(builder, path);
                    }
                }
                else
                {
                    string path;
                    while ((path = Console.ReadLine()) != null)
                    {
                        HandlePath(builder, path);
                    }
                }
                return 0;
            });
            app.Execute(args);
        }

        static void HandlePath(ASTBuilder builder, string path)
        {
            if (File.Exists(path))
            {
                var ast = builder.Build(path);
                using (var stdout = Console.OpenStandardOutput())
                {
                    ast.Save(stdout);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
