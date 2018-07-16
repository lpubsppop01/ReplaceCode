using Lpubsppop01.ReplaceCode.Base;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Lpubsppop01.ReplaceCode.Tests
{
    public class App_GetASTShould : IDisposable
    {
        const string TestDirectoryName = nameof(App_GetASTShould);

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
            Assert.Throws<ArgumentException>(() => app.GetAST(new string[0], 1, AppOptions.None));
            Assert.Throws<ArgumentNullException>(() => app.GetAST(null, 1, AppOptions.None));
            Assert.Throws<ArgumentException>(() => app.GetAST(new string[] { "" }, 0, AppOptions.None));
            Assert.Throws<ArgumentException>(() => app.GetAST(new string[] { "" }, -1, AppOptions.None));
        }

        [Fact]
        public void ReturnValidASTGivenSourceFilePath()
        {
            RefreshTestData();

            var ast = App.Current.GetAST(GetTestPath("HogeLib/HogeLib/Command.cs"));
            AssertNode(NodeKind.Enum, "Lpubsppop01.HogeLib.CommandKind", "public enum CommandKind", ast);
            AssertNode(NodeKind.Class, "Lpubsppop01.HogeLib.Command", "public class Command", ast);
            AssertNode(NodeKind.Method, "Lpubsppop01.HogeLib.Command.TryParse(string,out Command)", "public static bool TryParse(string text, out Command command)", ast);
        }

        [Fact]
        public void ReturnValidASTGivenSourceDirectoryPath()
        {
            RefreshTestData();

            var ast = App.Current.GetAST(GetTestPath("HogeLib/HogeLib"), GetTestPath("HogeLib/HogeLib.Tests"));
            AssertNode(NodeKind.Enum, "Lpubsppop01.HogeLib.CommandKind", "public enum CommandKind", ast);
            AssertNode(NodeKind.Class, "Lpubsppop01.HogeLib.Command", "public class Command", ast);
            AssertNode(NodeKind.Method, "Lpubsppop01.HogeLib.Command.TryParse(string,out Command)", "public static bool TryParse(string text, out Command command)", ast);
            AssertNode(NodeKind.Class, "Lpubsppop01.HogeLib.TextFileInfo", "class TextFileInfo", ast);
            AssertNode(NodeKind.Class, "Lpubsppop01.HogeLib.Tests.Command_TryParseShould", "public class Command_TryParseShould", ast);
        }

        static void AssertNode(NodeKind expectedKind, string expectedFullName, string expectedStartString, AST actualAST)
        {
            Node actualNode;
            Assert.True(actualAST.TryGetNode(expectedFullName, out actualNode));
            Assert.Equal(expectedKind, actualNode.Kind);
            Assert.Equal(expectedFullName.Split('.').Last(), actualNode.Name);
            Assert.True(actualNode.SourceIDs.Any());
            foreach (var src in actualNode.Sources(actualAST.SourceMap))
            {
                var srcFileText = File.ReadAllText(src.FilePath(actualAST.SourceMap));
                var srcText = srcFileText.Substring(src.ContentRange.Start, src.ContentRange.Length).Trim();
                Assert.StartsWith(expectedStartString, srcText);
            }
        }
    }
}
