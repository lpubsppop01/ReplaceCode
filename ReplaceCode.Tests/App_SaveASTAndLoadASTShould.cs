using Lpubsppop01.ReplaceCode.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Lpubsppop01.ReplaceCode.Tests
{
    public class App_SaveASTAndLoadASTShould : IDisposable
    {
        const string TestDirectoryName = nameof(App_SaveASTAndLoadASTShould);

        string JSONPath;
        AST srcAST;

        public App_SaveASTAndLoadASTShould()
        {
            JSONPath = Path.Join(Utility.HerePath, "ast.json");
            RefreshTestData();
            srcAST = App.Current.GetAST(GetTestPath("HogeLib/HogeLib/Command.cs"));
        }

        public void Dispose()
        {
            Utility.CleanTestData(TestDirectoryName);
            if (File.Exists(JSONPath)) File.Delete(JSONPath);
        }

        void RefreshTestData()
        {
            Utility.RefreshTestData(TestDirectoryName);
            if (File.Exists(JSONPath)) File.Delete(JSONPath);
        }

        string GetTestPath(string relativePath)
        {
            return Utility.GetTestPath(TestDirectoryName, relativePath);
        }

        class NodeComparer : IEqualityComparer<Node>
        {
            public bool Equals(Node x, Node y)
            {
                if (x.ID != y.ID) return false;
                if (x.Kind != y.Kind) return false;
                if (x.Name != y.Name) return false;
                if (!Enumerable.SequenceEqual(x.SourceIDs, y.SourceIDs)) return false;
                return true;
            }

            public int GetHashCode(Node obj)
            {
                var buf = new StringBuilder();
                buf.Append($"{obj.ID},{obj.Kind},{obj.Name}");
                foreach (var srcID in obj.SourceIDs)
                {
                    buf.Append($",{srcID}");
                }
                return buf.GetHashCode();
            }
        }

        class SourceComparer : IEqualityComparer<Source>
        {
            public bool Equals(Source x, Source y)
            {
                if (x.FileID != y.FileID) return false;
                if (x.ContentRange.Start != y.ContentRange.Start) return false;
                if (x.ContentRange.End != y.ContentRange.End) return false;
                if (x.Text != y.Text) return false;
                return true;
            }

            public int GetHashCode(Source obj)
            {
                var buf = new StringBuilder();
                buf.Append($"{obj.FileID},{obj.ContentRange.Start},{obj.ContentRange.End}");
                if (obj.Text != null)
                {
                    buf.Append(obj.Text);
                }
                return buf.GetHashCode();
            }
        }

        [Fact]
        public void WorkGivenJSONFilePath()
        {
            RefreshTestData();

            var app = App.Current;
            app.SaveAST(srcAST, JSONPath);
            var destAST = app.LoadAST(JSONPath);
            Assert.True(Enumerable.SequenceEqual(srcAST.Nodes, destAST.Nodes, new NodeComparer()));
            Assert.True(Enumerable.SequenceEqual(srcAST.SourceMap.FilePaths, destAST.SourceMap.FilePaths));
        }

        [Fact]
        public void ThrowExceptionGivenNull()
        {
            RefreshTestData();

            var app = App.Current;
            Assert.Throws<ArgumentNullException>(() => app.SaveAST(srcAST, null));
            Assert.Throws<ArgumentNullException>(() => app.SaveAST(null, JSONPath));
            Assert.Throws<ArgumentNullException>(() => app.LoadAST(null));
        }
    }
}
