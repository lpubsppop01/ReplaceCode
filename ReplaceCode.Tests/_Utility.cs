using Lpubsppop01.ReplaceCode.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace Lpubsppop01.ReplaceCode.Tests
{
    static class Utility
    {
        public static string HerePath;
        static string TestDataZipPath;

        static Utility()
        {
            HerePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            TestDataZipPath = Path.Combine(HerePath, "_TestData.zip");
        }

        public static void RefreshTestData(string testDirName)
        {
            CleanTestData(testDirName);
            var testDirPath = Path.Combine(HerePath, testDirName);
            ZipFile.ExtractToDirectory(TestDataZipPath, testDirPath);
        }

        public static string GetTestPath(string testDirName, string relativePath)
        {
            var testDirPath = Path.Combine(HerePath, testDirName);
            return Path.Combine(testDirPath, relativePath);
        }

        public static void CleanTestData(string testDirName)
        {
            var testDirPath = Path.Combine(HerePath, testDirName);
            if (!Directory.Exists(testDirPath)) return;
            Directory.Delete(testDirPath, recursive: true);
        }

        public static AST GetAST(this App app, params string[] paths)
        {
            return app.GetAST(paths, 1, AppOptions.None);
        }
    }
}
