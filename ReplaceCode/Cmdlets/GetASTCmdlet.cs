using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Lpubsppop01.ReplaceCode.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "AST")]
    public class GetASTCmdlet : Cmdlet
    {
        [Parameter(Mandatory = true, ValueFromRemainingArguments = true)]
        public string[] Paths { get; set; }

        [Parameter]
        public int ThreadCount { get; set; } = Environment.ProcessorCount;

        [Parameter]
        public bool AutoUpdate { get; set; } = true;

        protected override void ProcessRecord()
        {
            var options = AppOptions.UseCache;
            if (AutoUpdate) options |= AppOptions.AutoUpdate;
            var ast = App.Current.GetAST(Paths, ThreadCount, options);
            WriteObject(ast);
        }
    }
}
