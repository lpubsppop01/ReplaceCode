using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Lpubsppop01.ReplaceCode
{
    [DataContract]
    sealed class AppCache
    {
        #region Instance

        public static AppCache Current { get; private set; } = new AppCache();

        AppCache()
        {
            SourcePathToASTPath = new Dictionary<string, string>();
        }

        #endregion

        #region Properties

        [DataMember(Name = "sourcePathToASTPath")]
        public Dictionary<string, string> SourcePathToASTPath { get; private set; }

        [DataMember(Name = "lastASTPath")]
        public string LastASTPath { get; set; }

        #endregion

        #region Serialization

        public static void SaveCurrent()
        {
            var env = AppEnvironment.Current;
            Directory.CreateDirectory(Path.GetDirectoryName(env.CacheJsonPath));

            var serializer = new DataContractJsonSerializer(typeof(AppCache));
            var encoding = new UTF8Encoding(false);
            using (var stream = new FileStream(AppEnvironment.Current.CacheJsonPath, FileMode.Create))
            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, encoding, ownsStream: false, indent: true, indentChars: "  "))
            {
                serializer.WriteObject(writer, Current);
            }
        }

        public static void LoadCurrent()
        {
            if (File.Exists(AppEnvironment.Current.CacheJsonPath))
            {
                var serializer = new DataContractJsonSerializer(typeof(AppCache));
                var encoding = new UTF8Encoding(false);
                using (var stream = new FileStream(AppEnvironment.Current.CacheJsonPath, FileMode.Open))
                {
                    Current = serializer.ReadObject(stream) as AppCache;
                }
            }
            else
            {
                Current = new AppCache();
            }
        }

        #endregion
    }
}
