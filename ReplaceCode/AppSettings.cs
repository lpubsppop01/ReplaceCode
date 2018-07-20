using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Lpubsppop01.ReplaceCode
{
    class AppSettings
    {
        #region Instance

        public static BaseSettings Current { get; private set; } = new BaseSettings();

        AppSettings()
        {
        }

        #endregion

        #region Serialization

        public static void SaveCurrent(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            var serializer = new DataContractJsonSerializer(typeof(BaseSettings));
            var encoding = new UTF8Encoding(false);
            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, encoding, ownsStream: false, indent: true, indentChars: "  "))
            {
                serializer.WriteObject(writer, Current);
            }
        }

        public static void LoadCurrent(string path)
        {
            if (File.Exists(path))
            {
                var serializer = new DataContractJsonSerializer(typeof(BaseSettings));
                var encoding = new UTF8Encoding(false);
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    Current = serializer.ReadObject(stream) as BaseSettings;
                }
            }
            else
            {
                Current = new BaseSettings();
            }
        }

        #endregion
    }
}
