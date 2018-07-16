using System;
using System.Collections.Generic;
using System.Text;

namespace Lpubsppop01.ReplaceCode
{
    [Flags]
    public enum AppOptions
    {
        None = 0,
        UseCache = 1 << 1,
        AutoUpdate = 1 << 2
    }
}
