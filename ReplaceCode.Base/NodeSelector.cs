using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lpubsppop01.ReplaceCode.Base
{
    public sealed class NodeSelector
    {
        #region Constructor

        Node node;
        AST ast;

        public NodeSelector(Node node, AST ast)
        {
            this.node = node;
            this.ast = ast;
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

        public NodeSelector Parent() => new NodeSelector(node.Parent(ast), ast);

        IEnumerable<NodeSelector> _AncestorsOrSelf(Node self)
        {
            yield return new NodeSelector(self, ast);
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
            node.Children(ast).Select(c => new NodeSelector(c, ast));

        IEnumerable<NodeSelector> _DescendantsOrSelf(Node self)
        {
            yield return new NodeSelector(self, ast);
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

        static void OnEdited(AST ast)
        {
            SourceEditor.WriteUnparsedText(ast);
            ast.OnEdited();
        }

        #endregion
    }
}
