using Lpubsppop01.ReplaceCode.Base;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lpubsppop01.ReplaceCode.CSharp
{
    class ASTBuilder
    {
        string srcFilePath;
        AST destAST;

        public AST Build(string filePath)
        {
            srcFilePath = filePath;
            string content = File.ReadAllText(srcFilePath);
            var srcAST = CSharpSyntaxTree.ParseText(content);
            destAST = new AST();
            Traverse(destAST, destAST.Root, srcAST.GetRoot());
            return destAST;
        }

        void Traverse(AST destAST, Node destNode, SyntaxNode srcNode)
        {
            foreach (var srcChild in srcNode.ChildNodes())
            {
                if (srcChild is NamespaceDeclarationSyntax srcNamespace)
                {
                    var srcFullName = GetFullName(srcNamespace);
                    var destFullName = new List<string>();
                    var destNamespace = destNode;
                    for (int i = 0; i < srcFullName.Length; ++i)
                    {
                        destFullName.Add(srcFullName[i]);
                        Node destCurr;
                        if (!destAST.TryGetNode(destFullName, out destCurr))
                        {
                            destCurr = destAST.NewNode(NodeKind.Namespace, destFullName.Last(), destNamespace);
                        }
                        var destSrc = destAST.SourceMap.NewSource(srcFilePath, srcChild.FullSpan.Start, srcChild.FullSpan.End);
                        destCurr.SourceIDs.Add(destSrc.ID);
                        destNamespace = destCurr;
                    }
                    Traverse(destAST, destNamespace, srcNamespace);
                    continue;
                }

                var kind = GetNodeKind(srcChild);
                if (kind.HasValue)
                {
                    var fullName = GetFullName(srcChild);
                    if (!destAST.TryGetNode(fullName, out Node destChild))
                    {
                        destChild = destAST.NewNode(kind.Value, fullName.Last(), destNode);
                    }
                    var destSrc = destAST.SourceMap.NewSource(srcFilePath, srcChild.FullSpan.Start, srcChild.FullSpan.End);
                    destChild.SourceIDs.Add(destSrc.ID);
                    if (srcChild is ClassDeclarationSyntax)
                    {
                        Traverse(destAST, destChild, srcChild);
                    }
                }
            }
        }

        static NodeKind? GetNodeKind(SyntaxNode node)
        {
            if (node is NamespaceDeclarationSyntax)
            {
                return NodeKind.Namespace;
            }
            else if (node is ClassDeclarationSyntax)
            {
                return NodeKind.Class;
            }
            else if (node is MethodDeclarationSyntax)
            {
                return NodeKind.Method;
            }
            else if (node is FieldDeclarationSyntax)
            {
                return NodeKind.Field;
            }
            else if (node is PropertyDeclarationSyntax)
            {

            }
            else if (node is EnumDeclarationSyntax)
            {
                return NodeKind.Enum;
            }
            return null;
        }

        static string[] GetFullName(SyntaxNode node)
        {
            if (node is NamespaceDeclarationSyntax @namespace)
            {
                return @namespace.Name.ToString().Split('.');
            }
            else if (node is ClassDeclarationSyntax @class)
            {
                return GetFullName(@class.Parent).Concat(new[] { @class.Identifier.ValueText }).ToArray();
            }
            else if (node is MethodDeclarationSyntax method)
            {
                var parameterList = method.DescendantNodes().OfType<ParameterListSyntax>().First();
                var parameterStringBuf = new StringBuilder();
                parameterStringBuf.Append('(');
                foreach(var p in parameterList.Parameters)
                {
                    if (parameterStringBuf.Length > 1)
                    {
                        parameterStringBuf.Append(',');
                    }
                    var modifier = p.Modifiers.FirstOrDefault();
                    if (modifier != null && !string.IsNullOrEmpty(modifier.ValueText))
                    {
                        parameterStringBuf.Append(modifier.ValueText + " ");
                    }
                    parameterStringBuf.Append(p.Type.ToString());
                }
                parameterStringBuf.Append(')');
                var localName = method.Identifier.ValueText + parameterStringBuf.ToString();
                return GetFullName(method.Parent).Concat(new[] { localName }).ToArray();
            }
            else if (node is FieldDeclarationSyntax field)
            {
                var localName = field.DescendantNodes().OfType<VariableDeclaratorSyntax>().First().Identifier.ValueText;
                return GetFullName(field.Parent).Concat(new[] { localName }).ToArray();
            }
            else if (node is PropertyDeclarationSyntax property)
            {

            }
            else if (node is EnumDeclarationSyntax @enum)
            {
                return GetFullName(@enum.Parent).Concat(new[] { @enum.Identifier.ValueText }).ToArray();
            }
            return new string[0];
        }
    }
}
