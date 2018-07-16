using Lpubsppop01.ReplaceCode.Base;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Lpubsppop01.ReplaceCode.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "ASTRoot")]
    public class GetASTRootCmdlet : Cmdlet
    {
        [Parameter(Position = 1)]
        public AST AST { get; set; }

        protected override void ProcessRecord()
        {
            var actualAST = AST;
            if (actualAST == null)
            {
                if (AppCache.Current.LastASTPath == null) throw new ArgumentNullException();
                actualAST = AST.Load(AppCache.Current.LastASTPath);
            }
            var selector = new NodeSelector(actualAST.Root, actualAST);
            WriteObject(selector);
        }
    }
}
