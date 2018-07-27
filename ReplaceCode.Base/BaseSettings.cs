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
        #region Constructor

        public BaseSettings()
        {
            EditorCommand = "code -g";
            PassesLineNumberToEditor = true;
            Workspaces = new [] { new Workspace { Name = "Default", Paths = new string[0] } };
        }

        #endregion

        #region Properties

        [DataMember(Name = "editorCommand")]
        public string EditorCommand { get; set; }

        [DataMember(Name = "passesLineNumberToEditor")]
        public bool PassesLineNumberToEditor { get; set; }

        [DataMember(Name = "workspaces")]
        public Workspace[] Workspaces { get; set; }

        [DataMember(Name = "currentWorkspaceIndex")]
        public int CurrentWorkspaceIndex { get; set; }

        #endregion
    }

    [DataContract]
    public class Workspace
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "paths")]
        public string[] Paths { get; set; }
    }
}
