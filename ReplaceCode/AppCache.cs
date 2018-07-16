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
        public static AppCache Current { get; private set; } = new AppCache();

        AppCache()
        {
            SourcePathToASTPath = new Dictionary<string, string>();
        }

        [DataMember(Name = "sourcePathToASTPath")]
        public Dictionary<string, string> SourcePathToASTPath { get; private set; }

        [DataMember(Name = "lastASTPath")]
        public string LastASTPath { get; set; }

        public static void SaveCurrent()
        {
            var paths = AppPathSet.Current;
            Directory.CreateDirectory(Path.GetDirectoryName(paths.ASTCacheJsonPath));

            var serializer = new DataContractJsonSerializer(typeof(AppCache));
            var encoding = new UTF8Encoding(false);
            using (var stream = new FileStream(AppPathSet.Current.ASTCacheJsonPath, FileMode.Create))
            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, encoding, ownsStream: false, indent: true, indentChars: "  "))
            {
                serializer.WriteObject(writer, Current);
            }
        }

        public static void LoadCurrent()
        {
            if (File.Exists(AppPathSet.Current.ASTCacheJsonPath))
            {
                var serializer = new DataContractJsonSerializer(typeof(AppCache));
                var encoding = new UTF8Encoding(false);
                using (var stream = new FileStream(AppPathSet.Current.ASTCacheJsonPath, FileMode.Open))
                {
                    Current = serializer.ReadObject(stream) as AppCache;
                }
            }
            else
            {
                Current = new AppCache();
            }
        }
    }
}
