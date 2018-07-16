using Lpubsppop01.ReplaceCode.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Lpubsppop01.ReplaceCode.Tests
{
    public class App_UpdateASTShould : IDisposable
    {
        const string TestDirectoryName = nameof(App_UpdateASTShould);

        public void Dispose()
        {
            Utility.CleanTestData(TestDirectoryName);
        }

        void RefreshTestData()
        {
            Utility.RefreshTestData(TestDirectoryName);
        }

        string GetTestPath(string relativePath)
        {
            return Utility.GetTestPath(TestDirectoryName, relativePath);
        }

        [Fact]
        public void ThrowExceptionGivenInvalidArgument()
        {
            RefreshTestData();

            var app = App.Current;
            Assert.Throws<ArgumentNullException>(() => app.UpdateAST(null, 1, AppOptions.None));
            Assert.Throws<ArgumentException>(() => app.UpdateAST(new AST(), 0, AppOptions.None));
            Assert.Throws<ArgumentException>(() => app.UpdateAST(new AST(), -1, AppOptions.None));
        }

        [Fact]
        public void ReparseUpdatedNodes()
        {
            RefreshTestData();

            var app = App.Current;
            var ast = app.GetAST(GetTestPath("HogeLib/HogeLib/TextFileInfo.cs"));
            ast.TryGetNode("Lpubsppop01.HogeLib.TextFileInfo", out var node);
            var selector = new NodeSelector(node, ast);
            selector.Replace("TextFileInfo", "BinaryFileInfo").Exec();
            Assert.Equal(NodeKind.Unparsed, node.Kind);
            app.UpdateAST(ast, 1, AppOptions.None);
            var reparsedNode = ast.GetNode("Lpubsppop01.HogeLib.BinaryFileInfo");
            Assert.Equal(NodeKind.Class, reparsedNode.Kind);
        }
    }
}
