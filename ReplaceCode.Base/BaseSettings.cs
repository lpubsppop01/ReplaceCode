using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Lpubsppop01.ReplaceCode
{
    [DataContract]
    public class BaseSettings
    {
        #region Properties

        [DataMember(Name = "editor")]
        public string Editor { get; set; } = "code";

        #endregion
    }
}
