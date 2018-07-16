using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Lpubsppop01.ReplaceCode.Cmdlets
{
    [Cmdlet(VerbsCommon.Clear, "ASTCache")]
    public class ClearASTCacheCmdlet : Cmdlet
    {
        protected override void ProcessRecord()
        {
            App.Current.ClearASTCache();
        }
    }
}
