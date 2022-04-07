using Newtonsoft.Json;
using System;
using System.IO;

namespace Koala.Core
{
    public static class SistFileExtensions
    {
        public static T GetFromFile<T>(this string fileName, Func<T> defaultObjectProvider, string overrideStorageDirectoryName = default) where T : class
        {
            var storageDirectoryInfo = string.IsNullOrWhiteSpace(overrideStorageDirectoryName)
                ? fileName.GetDumpDirectory()
                : Directory.CreateDirectory(overrideStorageDirectoryName);

            var sampleJsonFilePath = Path.Combine(storageDirectoryInfo.FullName, fileName);

            if (!File.Exists(sampleJsonFilePath))
            {
                lock (GetFromFileLock)
                {
                    if (!File.Exists(sampleJsonFilePath))
                    {
                        var sample = defaultObjectProvider();
                        File.WriteAllText(sampleJsonFilePath, sample.Json(format: true));
                        return sample;
                    }
                }
            }

            var sampleJson = File.ReadAllText(sampleJsonFilePath);

            try
            {
                return sampleJson.Get<T>();
            }
            catch
            {
                return default(T);
            }
        }

        public static DirectoryInfo GetDumpDirectory(this object input, Guid? appId = default)
        {
            var rootDirectoryInfo = new DirectoryInfo("/tmp");

            if (!rootDirectoryInfo.Exists)
            {
                Directory.CreateDirectory(rootDirectoryInfo.FullName);
            }

            var dumpDirectoryName = appId != default ? $"dump-{appId.ToString().ToLower()}" : $"dump-{AppId.ToString().ToLower()}";

            var dumpDirectoryInfo = new DirectoryInfo(Path.Combine(rootDirectoryInfo.FullName, dumpDirectoryName));

            if (!dumpDirectoryInfo.Exists)
            {
                Directory.CreateDirectory(dumpDirectoryInfo.FullName);
            }

            return dumpDirectoryInfo;
        }

        private static readonly object GetFromFileLock = new object();

        private static readonly Guid AppId = Guid.NewGuid();
    }
}