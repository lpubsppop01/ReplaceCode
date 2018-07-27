using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lpubsppop01.ReplaceCode.Base
{
    public sealed class NodeSelector
    {
        #region Constructor

        Node node;
        AST ast;
        BaseSettings settings;

        public NodeSelector(Node node, AST ast, BaseSettings settings = null)
        {
            this.node = node;
            this.ast = ast;
            this.settings = settings != null ? settings : new BaseSettings();
        }

        #endregion

        #region Properties

        public string Name => node.Name;

        public string FullName() => string.Join('.', node.FullName(ast));

        public NodeKind Kind => node.Kind;

        public IEnumerable<string> Texts => node.Text(ast).SourceTexts(ast).Select(t => t.Text);

        public IEnumerable<string> FilePaths => node.Sources(ast.SourceMap).Select(s => s.FilePath(ast.SourceMap)).Distinct();

        #endregion

        #region Traversal Methods

        public NodeSelector Parent() => new NodeSelector(node.Parent(ast), ast, settings);

        IEnumerable<NodeSelector> _AncestorsOrSelf(Node self)
        {
            yield return new NodeSelector(self, ast, settings);
            var parent = self.Parent(ast);
            if (parent == null || parent == ast.Root) yield break;
            foreach (var parentResult in _AncestorsOrSelf(parent))
            {
                yield return parentResult;
            }
        }

        public IEnumerable<NodeSelector> AncestorsOrSelf() => _AncestorsOrSelf(node);

        public IEnumerable<NodeSelector> Ancestors() => _AncestorsOrSelf(node.Parent(ast));

        public IEnumerable<NodeSelector> Children() =>
            node.Children(ast).Select(c => new NodeSelector(c, ast, settings));

        IEnumerable<NodeSelector> _DescendantsOrSelf(Node self)
        {
            yield return new NodeSelector(self, ast, settings);
            foreach (var child in self.Children(ast))
            {
                foreach (var childResult in _DescendantsOrSelf(child))
                {
                    yield return childResult;
                }
            }
        }

        public IEnumerable<NodeSelector> DescendantsOrSelf() => _DescendantsOrSelf(node);

        public IEnumerable<NodeSelector> Descendants() =>
            node.Children(ast).SelectMany(child => _DescendantsOrSelf(child));

        #endregion

        #region Edit Methods

        public NodeDelete Delete() => new NodeDelete(node, ast, () => OnEdited(ast));

        public NodeReplace Replace(string pattern, string replacement) =>
            new NodeReplace(node, ast, () => OnEdited(ast), pattern, replacement);

        public NodeGenerateReplaced GenerateReplaced(string pattern, string replacement) =>
            new NodeGenerateReplaced(node, ast, () => OnEdited(ast), pattern, replacement);

        public void Open()
        {
            var paths = new List<string>();
            foreach (var src in node.Sources(ast.SourceMap))
            {
                if (settings.PassesLineNumberToEditor)
                {
                    var fileInfo = new TextFileInfo(src.FilePath(ast.SourceMap));
                    int line = fileInfo.ReadToEnd().Substring(0, src.ContentRange.Start).Split(fileInfo.NewLine).Count();
                    paths.Add($"{src.FilePath(ast.SourceMap)}:{line}");
                }
                else
                {
                    paths.Add(src.FilePath(ast.SourceMap));
                }
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("cmd.exe", "/c " + string.Join(' ', paths.Prepend(settings.EditorCommand)));
            }
            else
            {
                Process.Start("bash", "-c " + string.Join(' ', paths.Prepend(settings.EditorCommand)));

            }
        }

        static void OnEdited(AST ast)
        {
            SourceEditor.WriteUnparsedText(ast);
            ast.OnEdited();
        }

        #endregion
    }
}
