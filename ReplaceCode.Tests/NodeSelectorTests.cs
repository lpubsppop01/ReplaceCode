using Lpubsppop01.ReplaceCode.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Lpubsppop01.ReplaceCode.Tests
{
    public class NodeSelectorTests : IDisposable
    {
        const string TestDirectoryName = nameof(NodeSelectorTests);

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
        public void ParentShouldReturnCorrespondingNodeSelector()
        {
            RefreshTestData();

            var app = App.Current;
            var ast = app.GetAST(GetTestPath("HogeLib"));
            var selfNode = ast.GetNode("Lpubsppop01.HogeLib.Command.TryParse(string,out Command)");
            var expectedNode = ast.GetNode("Lpubsppop01.HogeLib.Command");
            var self = new NodeSelector(selfNode, ast);
            var actual = self.Parent();
            var expected = new NodeSelector(expectedNode, ast);
            Assert.Equal(expected.FullName(), actual.FullName());
        }

        [Fact]
        public void AncestorsOrSelfShouldReturnCorrespondingNodeSelectors()
        {
            RefreshTestData();

            var app = App.Current;
            var ast = app.GetAST(GetTestPath("HogeLib"));
            var selfNode = ast.GetNode("Lpubsppop01.HogeLib.Command.TryParse(string,out Command)");
            var expectedNodes = new List<Node> { selfNode };
            expectedNodes.Add(ast.GetNode("Lpubsppop01.HogeLib.Command"));
            expectedNodes.Add(ast.GetNode("Lpubsppop01.HogeLib"));
            expectedNodes.Add(ast.GetNode("Lpubsppop01"));
            var self = new NodeSelector(selfNode, ast);
            var actual = self.AncestorsOrSelf().ToArray();
            var expected = expectedNodes.Select(n => new NodeSelector(n, ast)).ToArray();
            Assert.True(Enumerable.Range(0, expected.Length).All(i => expected[i].FullName() == actual[i].FullName()));
        }

        [Fact]
        public void AncestorsShouldReturnCorrespondingNodeSelectors()
        {
            RefreshTestData();

            var app = App.Current;
            var ast = app.GetAST(GetTestPath("HogeLib"));
            var selfNode = ast.GetNode("Lpubsppop01.HogeLib.Command.TryParse(string,out Command)");
            var expectedNodes = new List<Node>();
            expectedNodes.Add(ast.GetNode("Lpubsppop01.HogeLib.Command"));
            expectedNodes.Add(ast.GetNode("Lpubsppop01.HogeLib"));
            expectedNodes.Add(ast.GetNode("Lpubsppop01"));
            var self = new NodeSelector(selfNode, ast);
            var actual = self.Ancestors().ToArray();
            var expected = expectedNodes.Select(n => new NodeSelector(n, ast)).ToArray();
            Assert.True(Enumerable.Range(0, expected.Length).All(i => expected[i].FullName() == actual[i].FullName()));
        }

        // TODO: Add tests for the other traversal methods

        [Fact]
        public void DeleteShouldUpdateASTAndSourceFiles()
        {
            RefreshTestData();

            var app = App.Current;
            var ast = app.GetAST(GetTestPath("HogeLib/HogeLib/TextFileInfo.cs"));
            var selfNode = ast.GetNode("Lpubsppop01.HogeLib.NewLineKind");
            var selector = new NodeSelector(selfNode, ast);
            int offset = selfNode.Sources(ast.SourceMap).First().ContentRange.Start;
            selector.Delete().Exec();

            Assert.Equal(NodeKind.Unparsed, selector.Kind);
            Assert.True(selector.Texts.All(t => t == ""));
            var nextSiblingNode = ast.GetNode("Lpubsppop01.HogeLib.NewLineUtility");
            Assert.Equal(offset, nextSiblingNode.Sources(ast.SourceMap).First().ContentRange.Start);

            var resultAst = app.GetAST(GetTestPath("HogeLib/HogeLib/TextFileInfo.cs"));
            Assert.False(resultAst.TryGetNode("Lpubsppop01.HogeLib.NewLineKind", out var _));
        }

        [Fact]
        public void ReplaceShouldUpdateASTAndSourceFiles()
        {
            RefreshTestData();

            var app = App.Current;
            var ast = app.GetAST(GetTestPath("HogeLib/HogeLib/TextFileInfo.cs"));
            var selfNode = ast.GetNode("Lpubsppop01.HogeLib.TextFileInfo");
            var selector = new NodeSelector(selfNode, ast);
            int offset = selfNode.Sources(ast.SourceMap).First().ContentRange.Start;
            selector.Replace("TextFileInfo", "BinaryFileInfo").Exec();

            Assert.Equal(NodeKind.Unparsed, selector.Kind);
            Assert.StartsWith("class BinaryFileInfo", selector.Texts.First().Trim());

            var resultAst = app.GetAST(GetTestPath("HogeLib/HogeLib/TextFileInfo.cs"));
            Assert.False(resultAst.TryGetNode("Lpubsppop01.HogeLib.TextFileInfo", out var _));
            Assert.True(resultAst.TryGetNode("Lpubsppop01.HogeLib.BinaryFileInfo", out var _));
        }

        [Fact]
        public void GenerateReplacedShouldUpdateASTAndSourceFiles()
        {
            RefreshTestData();

            var app = App.Current;
            var ast = app.GetAST(GetTestPath("HogeLib/HogeLib/TextFileInfo.cs"));
            var selfNode = ast.GetNode("Lpubsppop01.HogeLib.TextFileInfo");
            var selector = new NodeSelector(selfNode, ast);
            int offset = selfNode.Sources(ast.SourceMap).First().ContentRange.Start;
            selector.GenerateReplaced("TextFileInfo", "BinaryFileInfo").Exec();

            // TODO: Add assertion with NextSibling

            var resultAst = app.GetAST(GetTestPath("HogeLib/HogeLib/TextFileInfo.cs"));
            Assert.True(resultAst.TryGetNode("Lpubsppop01.HogeLib.TextFileInfo", out var _));
            Assert.True(resultAst.TryGetNode("Lpubsppop01.HogeLib.BinaryFileInfo", out var _));
        }
    }
}
