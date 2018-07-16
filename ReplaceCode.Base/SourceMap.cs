using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Lpubsppop01.ReplaceCode.Base
{
    [DataContract]
    public class SourceMap
    {
        #region Constructor

        const int FirstID = 1;

        public SourceMap()
        {
            IDToSource = new Dictionary<int, Source>();
            AssignedSourceIDRange = new IntRange(FirstID, FirstID);
            IDToFilePath = new Dictionary<int, string>();
            AssignedFileIDRange = new IntRange(FirstID, FirstID);
        }

        #endregion

        #region Properties

        [DataMember(Name = "idToSource")]
        internal Dictionary<int, Source> IDToSource { get; private set; }

        [DataMember(Name = "assignedSourceIDRange")]
        IntRange AssignedSourceIDRange;

        [DataMember(Name = "idToFilePath")]
        internal Dictionary<int, string> IDToFilePath { get; private set; }

        Dictionary<string, int> m_FilePathToID;
        internal Dictionary<string, int> FilePathToID
        {
            get
            {
                if (m_FilePathToID == null)
                {
                    m_FilePathToID = IDToFilePath.ToDictionary(p => p.Value, p => p.Key);
                }
                return m_FilePathToID;
            }
        }

        [DataMember(Name = "assignedFileIDRange")]
        IntRange AssignedFileIDRange;

        public ICollection<Source> Sources => IDToSource.Values;

        public ICollection<string> FilePaths => IDToFilePath.Values;

        public static Type[] ComponentTypes => new Type[] {
            typeof(Source), typeof(IntRange)
        };

        #endregion

        #region Manipulations

        public bool TryGetSource(int id, out Source src)
        {
            return IDToSource.TryGetValue(id, out src);
        }

        public Source GetSource(int id)
        {
            if (!TryGetSource(id, out var src)) throw new ArgumentException();
            return src;
        }

        public Source NewSource(string filePath, int contentStart, int contentEnd)
        {
            return NewSource(filePath, new IntRange(contentStart, contentEnd));
        }

        public Source NewSource(string filePath, IntRange contentRange)
        {
            if (!FilePathToID.TryGetValue(filePath, out var srcFileID))
            {
                srcFileID = AssignedFileIDRange.End++;
                FilePathToID[filePath] = srcFileID;
                IDToFilePath[srcFileID] = filePath;
            }
            var src = new Source(srcFileID, contentRange)
            {
                ID = AssignedSourceIDRange.End++
            };
            IDToSource[src.ID] = src;
            return src;
        }

        #endregion
    }

    [DataContract]
    public class Source
    {
        #region Constructor

        public Source(int fileID, IntRange contentRange)
        {
            FileID = fileID;
            ContentRange = contentRange;
            Text = null;
        }

        protected Source(Source src)
        {
            FileID = src.FileID;
            ContentRange = src.ContentRange;
            Text = src.Text;
        }

        #endregion

        #region Properties

        [DataMember(Name = "id")]
        public int ID { get; internal set; }

        [DataMember(Name = "fileID")]
        public int FileID { get; internal set; }

        public string FilePath(SourceMap map) => map.IDToFilePath[FileID];

        [DataMember(Name = "contentRange")]
        public IntRange ContentRange;

        public double? SubOrder { get; internal set; }
        public string Text;

        #endregion
    }
}
