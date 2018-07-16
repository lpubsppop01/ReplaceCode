using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lpubsppop01.ReplaceCode.Base
{
    public class NodeText
    {
        #region Constructor

        Node node;
        AST ast;

        public NodeText(Node node, AST ast)
        {
            this.node = node;
            this.ast = ast;
        }

        #endregion

        #region Manipulations

        public IEnumerable<SourceText> SourceTexts(AST ast) // TODO: Remove this argument
        {
            foreach (int srcID in node.SourceIDs)
            {
                var text = new SourceText(srcID, ast.SourceMap);
                yield return text;
            }
        }

        public void Replace(string pattern, string replacement)
        {
            if (node.Kind != NodeKind.Unparsed) throw new InvalidOperationException();
            foreach (var text in SourceTexts(ast))
            {
                text.Text = text.Text.Replace(pattern, replacement);
            }
        }

        public void Clear()
        {
            if (node.Kind != NodeKind.Unparsed) throw new InvalidOperationException();
            foreach (var text in SourceTexts(ast))
            {
                text.Text = "";
            }
        }

        #endregion
    }

    public class SourceText
    {
        #region Constructor

        int srcID;
        SourceMap map;

        public SourceText(int srcID, SourceMap map)
        {
            this.srcID = srcID;
            this.map = map;
        }

        #endregion

        #region Property

        public Source Source => map.GetSource(srcID);

        public string Text
        {
            get
            {
                if (Source.Text == null)
                {
                    var filePath = map.IDToFilePath[Source.FileID];
                    var fileText = new TextFileInfo(filePath).ReadToEnd();
                    var src = Source;
                    src.Text = fileText.Substring(Source.ContentRange.Start, Source.ContentRange.Length);
                }
                return Source.Text;
            }
            set
            {
                var src = Source;
                src.Text = value;
            }
        }

        #endregion
    }
}
