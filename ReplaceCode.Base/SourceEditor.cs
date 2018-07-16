using Lpubsppop01.ReplaceCode.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lpubsppop01.ReplaceCode.Base
{
    static class SourceEditor
    {
        public static void WriteUnparsedText(this AST ast)
        {
            var pathToEdit = new Dictionary<string, SortedSet<SourceEditUnit>>();
            foreach (var node in ast.Nodes.Where(n => n.Kind == NodeKind.Unparsed))
            {
                //Console.WriteLine($"{node.Kind.ToString()} {node.Name}");
                foreach (var srcText in node.Text(ast).SourceTexts(ast))
                {
                    if (!pathToEdit.TryGetValue(srcText.Source.FilePath(ast.SourceMap), out var value))
                    {
                        pathToEdit[srcText.Source.FilePath(ast.SourceMap)] = value = new SortedSet<SourceEditUnit>(new SourceEditUnitComparer());
                    }
                    value.Add(new SourceEditUnit
                    {
                        Source = srcText.Source,
                        Text = srcText.Text
                    });
                }
            }

            foreach (var pair in pathToEdit)
            {
                var filePath = pair.Key;
                var fileID = ast.SourceMap.FilePathToID[filePath];
                var sortedEditItems = pair.Value;
                foreach (var editItem in sortedEditItems.Reverse())
                {
                    var fileInfo = new TextFileInfo(filePath);
                    var allText = fileInfo.ReadToEnd();
                    var headPart = allText.Substring(0, editItem.Source.ContentRange.Start);
                    var targetPart = allText.Substring(editItem.Source.ContentRange.Start, editItem.Source.ContentRange.End - editItem.Source.ContentRange.Start);
                    var footPart = allText.Substring(editItem.Source.ContentRange.End);
                    //Console.WriteLine($"  {filePath}:{editItem.Source.ContentRange.Start}:{editItem.Source.ContentRange.End}");
                    //Console.WriteLine(editItem.Text);

                    using (var writer = new StreamWriter(filePath, /* append: */ false, fileInfo.Encoding))
                    {
                        writer.Write(headPart);
                        writer.Write(editItem.Text);
                        writer.Write(footPart);
                    }

                    int newEnd = editItem.Source.ContentRange.Start + editItem.Text.Length;
                    int offset = newEnd - editItem.Source.ContentRange.End;
                    editItem.Source.ContentRange.End = newEnd;
                    foreach(var src in ast.SourceMap.Sources)
                    {
                        if (src == editItem.Source) continue;
                        if (src.FileID != fileID) continue;
                        if (src.ContentRange.Start < editItem.Source.ContentRange.Start) continue;
                        if (src.ContentRange.Start == editItem.Source.ContentRange.Start &&
                            (src.SubOrder ?? 0) < (editItem.Source.SubOrder ?? 0)) continue;
                        src.ContentRange.Start += offset;
                        src.ContentRange.End += offset;
                    }
                }
            }

            ast.OnEdited();
        }

        class SourceEditUnit
        {
            public Source Source;
            public string Text;
        }

        class SourceEditUnitComparer : IComparer<SourceEditUnit>
        {
            public int Compare(SourceEditUnit x, SourceEditUnit y)
            {
                // Ignore file path
                if (x.Source.ContentRange.End < y.Source.ContentRange.End) return -1;
                if ((x.Source.SubOrder ?? 0) < (y.Source.SubOrder ?? 0)) return -1;
                return 0;
            }
        }
    }
}
