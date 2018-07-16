using Lpubsppop01.ReplaceCode.Base;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Lpubsppop01.ReplaceCode.Cmdlets
{
    [Cmdlet(VerbsData.Update, "AST")]
    public class UpdateASTCmdlet : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 1)]
        public AST AST { get; set; }

        [Parameter]
        public int ThreadCount { get; set; } = Environment.ProcessorCount;

        protected override void ProcessRecord()
        {
            var options = AppOptions.UseCache;
            var ast = App.Current.UpdateAST(AST, ThreadCount, options);
            WriteObject(ast);
        }
    }
}
