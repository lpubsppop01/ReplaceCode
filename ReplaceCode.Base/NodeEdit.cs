using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lpubsppop01.ReplaceCode.Base
{
    public abstract class NodeEdit
    {
        protected Node node;
        protected AST ast;
        protected Action onEdited;

        protected NodeEdit(Node node, AST ast, Action onEdited)
        {
            this.node = node;
            this.ast = ast;
            this.onEdited = onEdited;
        }

        string m_Preview;
        public string Preview
        {
            get
            {
                if (m_Preview == null)
                {
                    m_Preview = GetPreview();
                }
                return m_Preview;
            }
        }

        protected abstract string GetPreview();

        public abstract void Exec();

        protected static void Unparse(Node node, AST ast)
        {
            if (node.Kind == NodeKind.Unparsed) throw new InvalidOperationException();
            node.Kind = NodeKind.Unparsed;
            foreach (var id in node.ChildIDs)
            {
                ast.IDToNode.Remove(id);
            }
            node.ChildIDs.Clear();
        }
    }

    public class NodeDelete : NodeEdit
    {
        public NodeDelete(Node node, AST ast, Action onEdited)
            : base(node, ast, onEdited)
        {
        }

        public override void Exec()
        {
            if (node.Kind != NodeKind.Unparsed)
            {
                Unparse(node, ast);
            }
            node.Text(ast).Clear();
            onEdited();
        }

        protected override string GetPreview()
        {
            var buf = new StringBuilder();
            buf.AppendLine($"Delete {string.Join('.', node.FullName(ast))}:");
            foreach (var src in node.Sources(ast.SourceMap))
            {
                buf.AppendLine($"  {src.FilePath(ast.SourceMap)}:");
                var srcText = new SourceText(src.ID, ast.SourceMap);
                var srcLines = srcText.Text.Split(Environment.NewLine);
                foreach(var srcLine in srcLines)
                {
                    buf.AppendLine($"    -{srcLine}");
                }
            }
            return buf.ToString();
        }
    }

    public class NodeReplace : NodeEdit
    {
        string pattern;
        string replacement;

        public NodeReplace(Node node, AST ast, Action onEdited, string pattern, string replacement)
            : base(node, ast, onEdited)
        {
            this.pattern = pattern;
            this.replacement = replacement;
        }

        public override void Exec()
        {
            if (node.Kind != NodeKind.Unparsed)
            {
                Unparse(node, ast);
            }
            node.Text(ast).Replace(pattern, replacement);
            onEdited();
        }

        protected override string GetPreview()
        {
            var buf = new StringBuilder();
            buf.AppendLine($"Replace {string.Join('.', node.FullName(ast))}:");
            foreach (var src in node.Sources(ast.SourceMap))
            {
                buf.AppendLine($"  {src.FilePath(ast.SourceMap)}:");
                var srcText = new SourceText(src.ID, ast.SourceMap);
                var srcLines = srcText.Text.Split(Environment.NewLine);
                foreach (var line in srcLines)
                {
                    buf.AppendLine($"    -{line}");
                }
                var replacedText = srcText.Text.Replace(pattern, replacement);
                var replacedLines = replacedText.Split(Environment.NewLine);
                foreach (var line in replacedLines)
                {
                    buf.AppendLine($"    +{line}");
                }
            }
            return buf.ToString();
        }
    }

    public class NodeGenerateReplaced : NodeEdit
    {
        string pattern;
        string replacement;

        public NodeGenerateReplaced(Node node, AST ast, Action onEdited, string pattern, string replacement)
            : base(node, ast, onEdited)
        {
            this.pattern = pattern;
            this.replacement = replacement;
        }

        public override void Exec()
        {
            var copy = ast.NewNode(NodeKind.Unparsed, node.Name.Replace(pattern, replacement), node.Parent(ast));
            foreach (var srcText in node.Text(ast).SourceTexts(ast))
            {
                var filePath = srcText.Source.FilePath(ast.SourceMap);
                var copyContentRange = new IntRange(srcText.Source.ContentRange.End, srcText.Source.ContentRange.End);
                var copySrc = ast.SourceMap.NewSource(filePath, copyContentRange);
                copySrc.Text = srcText.Text;
                copySrc.ContentRange.Start = copySrc.ContentRange.End;
                var samePosSrcs = ast.SourceMap.Sources.Where(s => s.FileID == copySrc.FileID && s.ContentRange.Start == copySrc.ContentRange.Start).ToArray();
                var subOrder = samePosSrcs.Max(s => s.SubOrder ?? 0.0) + 1;
                copySrc.SubOrder = subOrder;
                copy.SourceIDs.Add(copySrc.ID);
            }
            copy.Text(ast).Replace(pattern, replacement);
            onEdited();
        }

        protected override string GetPreview()
        {
            var buf = new StringBuilder();
            buf.AppendLine($"Generate replaced {string.Join('.', node.FullName(ast))}:");
            foreach (var src in node.Sources(ast.SourceMap))
            {
                buf.AppendLine($"  {src.FilePath(ast.SourceMap)}:");
                var srcText = new SourceText(src.ID, ast.SourceMap);
                var replacedText = srcText.Text.Replace(pattern, replacement);
                var replacedLines = replacedText.Split(Environment.NewLine);
                foreach (var line in replacedLines)
                {
                    buf.AppendLine($"    +{line}");
                }
            }
            return buf.ToString();
        }
    }
}
