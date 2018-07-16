using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Lpubsppop01.ReplaceCode.Base
{
    [DataContract]
    public class AST
    {
        #region Constructor

        const int RootID = 1;

        public AST()
        {
            IDToNode = new Dictionary<int, Node> { [RootID] = new Node { ID = RootID, Kind = NodeKind.Namespace, Name = "" } };
            AssignedNodeIDRange = new IntRange(RootID, RootID + 1);
            SourceMap = new SourceMap();
        }

        #endregion

        #region Properties

        [DataMember(Name = "targetPaths")]
        public string[] TargetPaths { get; set; }

        [DataMember(Name = "timestamp")]
        public long Timestamp { get; set; }

        [DataMember(Name = "idToNode")]
        internal Dictionary<int, Node> IDToNode { get; private set; }

        [DataMember(Name = "assignedNodeIDRange")]
        IntRange AssignedNodeIDRange;

        [DataMember(Name = "sourceMap")]
        public SourceMap SourceMap { get; private set; }

        public Node Root => IDToNode[RootID];
        public ICollection<Node> Nodes => IDToNode.Values;

        public static Type[] ComponentTypes => new Type[] {
            typeof(Node), typeof(NodeKind), typeof(Source), typeof(IntRange)
        };

        public event EventHandler Edited;

        internal void OnEdited()
        {
            Edited?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Serialization

        public void Save(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var stream = new FileStream(path, FileMode.Create))
            {
                Save(stream);
            }
        }

        public void Save(Stream stream)
        {
            var serializer = new DataContractJsonSerializer(typeof(AST), ComponentTypes);
            var encoding = new UTF8Encoding(false);
            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, encoding, ownsStream: false, indent: true, indentChars: "  "))
            {
                serializer.WriteObject(writer, this);
            }
        }

        public static AST Load(string path)
        {
            var serializer = new DataContractJsonSerializer(typeof(AST), AST.ComponentTypes);
            var encoding = new UTF8Encoding(false);
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return serializer.ReadObject(stream) as AST;
            }
        }

        #endregion

        #region Manipulations

        public bool TryGetNode(string fullName, out Node node)
        {
            return TryGetNode(fullName.Split('.'), out node);
        }

        public bool TryGetNode(IList<string> fullName, out Node node)
        {
            node = Root as Node;
            var nameBuf = fullName.ToList();
            while (nameBuf.Any())
            {
                var name = nameBuf.First();
                nameBuf.RemoveAt(0);
                var child = node.Children(this).FirstOrDefault(c => c.Name == name);
                if (child == null) return false;
                node = child;
            }
            return true;
        }

        public Node GetNode(string fullName)
        {
            if (!TryGetNode(fullName, out var node)) throw new ArgumentException();
            return node;
        }

        public Node NewNode(NodeKind kind, string name, Node parent)
        {
            var newNode = new Node
            {
                ID = AssignedNodeIDRange.End++,
                Kind = kind,
                Name = name,
                ParentID = parent.ID
            };
            parent.ChildIDs.Add(newNode.ID);
            IDToNode[newNode.ID] = newNode;
            return newNode;
        }

        public void Merge(AST anotherAST)
        {
            void mergeNode(Node node, Node anotherNode)
            {
                var unparsedChildren = node.Children(this).Where(n => n.Kind == NodeKind.Unparsed).ToArray();
                foreach (var child in unparsedChildren)
                {
                    node.ChildIDs.Remove(child.ID);
                }

                foreach (var anotherChild in anotherNode.Children(anotherAST))
                {
                    Node child;
                    if (!TryGetNode(anotherChild.FullName(anotherAST), out child))
                    {
                        child = anotherChild.Clone();
                        child.ID = AssignedNodeIDRange.End++;
                        child.ParentID = node.ID;
                        child.ChildIDs.Clear();
                        node.ChildIDs.Add(child.ID); // TODO: Consider order in source
                        IDToNode[child.ID] = child;

                        child.SourceIDs.Clear();
                        foreach (var s in anotherChild.Sources(anotherAST.SourceMap))
                        {
                            var mergedSrc = SourceMap.NewSource(s.FilePath(anotherAST.SourceMap), s.ContentRange);
                            child.SourceIDs.Add(mergedSrc.ID);
                        }
                    }
                    mergeNode(child, anotherChild);
                }
            }
            mergeNode(Root, anotherAST.Root);
        }

        public void TrimUnparsed()
        {

        }

        #endregion
    }

    [DataContract]
    public enum NodeKind
    {
        [EnumMember(Value = "unparsed")]
        Unparsed,
        [EnumMember(Value = "namespace")]
        Namespace,
        [EnumMember(Value = "class")]
        Class,
        [EnumMember(Value = "method")]
        Method,
        [EnumMember(Value = "field")]
        Field,
        [EnumMember(Value = "enum")]
        Enum
    }

    [DataContract]
    public sealed class Node
    {
        #region Constructors

        internal Node()
        {
            ChildIDs = new List<int>();
            SourceIDs = new List<int>();
        }

        Node(Node src)
        {
            ID = src.ID;
            Kind = src.Kind;
            Name = src.Name;
            ParentID = src.ParentID;
            ChildIDs = new List<int>(src.ChildIDs);
            SourceIDs = new List<int>(src.SourceIDs);
        }

        #endregion

        #region Properties

        [DataMember(Name = "id")]
        public int ID { get; internal set; }

        public NodeKind Kind { get; internal set; }

        [DataMember(Name = "kind")]
        String KindAsString
        {
            get
            {
                return Kind.ToString().ToLower();
            }
            set
            {
                Kind = (NodeKind)Enum.Parse(typeof(NodeKind), value, ignoreCase: true);
            }
        }

        [DataMember(Name = "name")]
        public string Name { get; internal set; }

        public string[] FullName(AST ast)
        {
            var fullName = new List<string>();
            var curr = this;
            while (curr != null && !string.IsNullOrEmpty(curr.Name))
            {
                fullName.Insert(0, curr.Name);
                curr = curr.Parent(ast);
            }
            return fullName.ToArray();
        }

        [DataMember(Name = "parentID")]
        public int ParentID { get; internal set; }

        public Node Parent(AST ast)
        {
            if (ast.IDToNode.TryGetValue(ParentID, out var parent)) return parent;
            return null;
        }

        public List<int> ChildIDs { get; private set; }

        [DataMember(Name = "childIDs")]
        int[] ChildIDAsArray
        {
            get
            {
                return ChildIDs.ToArray();
            }
            set
            {
                if (ChildIDs == null)
                {
                    ChildIDs = new List<int>();
                }
                else
                {
                    ChildIDs.Clear();
                }
                foreach (var v in value)
                {
                    ChildIDs.Add(v);
                }
            }
        }

        public IEnumerable<Node> Children(AST ast) => ChildIDs.Select(id => ast.IDToNode[id]);

        public List<int> SourceIDs { get; private set; }

        [DataMember(Name = "sourceIDs")]
        int[] SourceIDsAsArray
        {
            get
            {
                return SourceIDs.ToArray();
            }
            set
            {
                if (SourceIDs == null)
                {
                    SourceIDs = new List<int>();
                }
                else
                {
                    SourceIDs.Clear();
                }
                foreach (var v in value)
                {
                    SourceIDs.Add(v);
                }
            }
        }

        public IEnumerable<Source> Sources(SourceMap map) => SourceIDs.Select(id => map.GetSource(id));

        public NodeText Text(AST ast) => new NodeText(this, ast);

        #endregion

        #region Clone

        internal Node Clone() => new Node(this);

        #endregion
    }

    [DataContract]
    public struct IntRange
    {
        #region Constructor

        public IntRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        #endregion

        #region Properties

        [DataMember(Name = "start")]
        public int Start;

        [DataMember(Name = "end")]
        public int End;

        public int Length => End - Start;

        #endregion
    }
}
