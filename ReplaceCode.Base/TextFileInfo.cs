using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lpubsppop01.ReplaceCode.Base
{
    public class TextFileInfo
    {
        static TextFileInfo()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        static (Encoding encoding, string newLine) DetectEncoding(string path)
        {
            var file = new FileInfo(path);
            using (var reader = new Hnx8.ReadJEnc.FileReader(file))
            {
                var charCode = reader.Read(file);
                if (reader.Text == null) return (null, null);
                var encoding = charCode.GetEncoding();
                var newLine = (reader.Text.DetectNewLineKind() ?? NewLineKind.CRLF).ToNewLineString();
                return (encoding, newLine);
            }
        }

        public TextFileInfo(string path)
        {
            Path = path;
            (var encoding, var newLine) = DetectEncoding(path);
            Encoding = encoding;
            NewLineKind = newLine.ToNewLineKind();
        }

        public string Path { get; private set; }
        public Encoding Encoding { get; private set; }
        public NewLineKind NewLineKind { get; private set; }
        public string NewLine => NewLineKind.ToNewLineString();
        public bool IsValid => Encoding != null;

        public List<string> ReadAllLines()
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(Path, Encoding))
            {
                lines.AddRange(reader.ReadToEnd().Split(NewLine));
            }
            return lines;
        }

        public string ReadToEnd()
        {
            using (var reader = new StreamReader(Path, Encoding))
            {
                return reader.ReadToEnd();
            }
        }
    }

    public enum NewLineKind
    {
        CRLF, LF
    }

    public static class NewLineUtility
    {
        public static string ToNewLineString(this NewLineKind kind)
        {
            switch (kind)
            {
                default:
                case NewLineKind.CRLF: return "\r\n";
                case NewLineKind.LF: return "\n";
            }
        }

        public static NewLineKind ToNewLineKind(this string newLineStr)
        {
            if (newLineStr == "\r\n")
            {
                return NewLineKind.CRLF;
            }
            else if (newLineStr == "\n")
            {
                return NewLineKind.LF;
            }
            throw new ArgumentException("The passed newLineStr value is not new line characters.");
        }

        public static NewLineKind? DetectNewLineKind(this string str)
        {
            int iLF = str.IndexOf('\n');
            if (iLF == -1) return NewLineKind.CRLF;
            if (iLF == 0) return NewLineKind.LF;
            return (str[iLF - 1] == '\r') ? NewLineKind.CRLF : NewLineKind.LF;
        }
    }
}
