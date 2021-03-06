﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Lpubsppop01.ReplaceCode
{
    class AppEnvironment
    {
        #region Instance

        public static AppEnvironment Current { get; private set; } = new AppEnvironment();

        #endregion

        #region Properties

        string m_Here;
        string Here
        {
            get
            {
                if (m_Here == null)
                {
                    m_Here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                return m_Here;
            }
        }

        string m_CSharpParserDebugDLLPath;
        string CSharpParserDebugDLLPath
        {
            get
            {
                if (m_CSharpParserDebugDLLPath == null)
                {
                    const string pathFromHere = @"../../../../ReplaceCode.CSharp/bin/Debug/netcoreapp2.1/Lpubsppop01.ReplaceCode.CSharp.dll";
                    m_CSharpParserDebugDLLPath = Path.Combine(Here, pathFromHere);
                }
                return m_CSharpParserDebugDLLPath;
            }
        }

        string m_CSharpParserPublishExePath;
        string CSharpParserPublishExePath
        {
            get
            {
                if (m_CSharpParserPublishExePath == null)
                {
                    const string pathFromHere = @"Lpubsppop01.ReplaceCode.CSharp.exe";
                    m_CSharpParserPublishExePath = Path.Combine(Here, pathFromHere);
                }
                return m_CSharpParserPublishExePath;
            }
        }

        string m_CSharpParserPath;
        public string CSharpParserPath
        {
            get
            {
                if (m_CSharpParserPath == null)
                {
                    if (File.Exists(CSharpParserDebugDLLPath))
                    {
                        m_CSharpParserPath = CSharpParserDebugDLLPath;
                    }
                    else if (File.Exists(CSharpParserPublishExePath))
                    {
                        m_CSharpParserPath = CSharpParserPublishExePath;
                    }
                }
                return m_CSharpParserPath;
            }
        }

        public string MyLocalAppDataPath
        {
            get
            {
                var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(localAppDataPath, "Lpubsppop01/ReplaceCode");
            }
        }

        public string CacheJsonPath => Path.Combine(MyLocalAppDataPath, "cache.json");

        public string SettingsJsonPath => Path.Combine(MyLocalAppDataPath, "paths.json");

        #endregion
    }
}
