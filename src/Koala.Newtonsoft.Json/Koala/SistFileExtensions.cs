using Newtonsoft.Json;
using System;
using System.IO;

namespace Koala.Core
{
    public static class SistFileExtensions
    {
        public static Stream GetFromFile(this string fileName, Func<Stream> defaultProvider, string storageDirectoryRelativePath = default, bool? forceOverwrite = default, bool? useDefaultDirectory = default)
        {
            var fileInfo = fileName.GetStorageFile(storageDirectoryRelativePath, useDefaultDirectory);
            var filePath = fileInfo.FullName;

            if (!File.Exists(filePath) || (forceOverwrite ?? false))
            {
                lock (GetFileStreamLock)
                {
                    if (!File.Exists(filePath) || (forceOverwrite ?? false))
                    {
                        var dataStream = defaultProvider();

                        var ms = new MemoryStream();
                        dataStream.CopyTo(dataStream);
                        var data = ms.ToArray();

                        File.WriteAllBytes(filePath, data);
                    }
                }
            }

            var dataBytes = File.ReadAllBytes(filePath);
            return new MemoryStream(dataBytes);
        }

        public static string GetFromFile(this string fileName, Func<string> defaultProvider, string storageDirectoryRelativePath = default, bool? forceOverwrite = default, bool? useDefaultDirectory = default)
        {
            var fileInfo = fileName.GetStorageFile(storageDirectoryRelativePath, useDefaultDirectory);
            var filePath = fileInfo.FullName;

            if (!File.Exists(filePath) || (forceOverwrite ?? false))
            {
                lock (GetFileStringLock)
                {
                    if (!File.Exists(filePath) || (forceOverwrite ?? false))
                    {
                        var data = defaultProvider();
                        File.WriteAllText(filePath, data);
                        return data;
                    }
                }
            }

            return File.ReadAllText(filePath);
        }

        public static byte[] GetFromFile(this string fileName, Func<byte[]> defaultProvider, string storageDirectoryRelativePath = default, bool? forceOverwrite = default, bool? useDefaultDirectory = default)
        {
            var fileInfo = fileName.GetStorageFile(storageDirectoryRelativePath, useDefaultDirectory);
            var filePath = fileInfo.FullName;

            if (!File.Exists(filePath) || (forceOverwrite ?? false))
            {
                lock (GetFileBinaryLock)
                {
                    if (!File.Exists(filePath) || (forceOverwrite ?? false))
                    {
                        var data = defaultProvider();
                        File.WriteAllBytes(filePath, data);
                        return data;
                    }
                }
            }

            return File.ReadAllBytes(filePath);
        }

        public static T GetFromFile<T>(this string fileName, Func<T> defaultProvider, string storageDirectoryRelativePath = default, bool? forceOverwrite = default, bool? useDefaultDirectory = default) where T : class
        {
            var sampleJsonFileInfo = fileName.GetStorageFile(storageDirectoryRelativePath, useDefaultDirectory);
            var sampleJsonFilePath = sampleJsonFileInfo.FullName;

            if (!File.Exists(sampleJsonFilePath) || (forceOverwrite ?? false))
            {
                lock (GetFromFileLock)
                {
                    if (!File.Exists(sampleJsonFilePath) || (forceOverwrite ?? false))
                    {
                        var sample = defaultProvider();
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

        public static FileInfo GetStorageFile(this string fileName, string storageDirectoryRelativePath = default, bool? useDefaultDirectory = default)
        {
            var storageDirectoryInfo = storageDirectoryRelativePath.GetStorageDirectory(useDefaultDirectory);
            var storageFileInfo = Path.Combine(storageDirectoryInfo.FullName, fileName);
            return new FileInfo(storageFileInfo);
        }

        public static DirectoryInfo GetStorageDirectory(this string storageDirectoryRelativePath, bool? useDefaultDirectory = default)
        {
            var rootDirectoryInfo = new DirectoryInfo(string.IsNullOrWhiteSpace(KoalaGlobals.AppFilesRoot) ? "/tmp" : KoalaGlobals.AppFilesRoot.Trim());
            var defaultDirectoryName = "default";

            if (!rootDirectoryInfo.Exists)
            {
                Directory.CreateDirectory(rootDirectoryInfo.FullName);
            }

            if (!string.IsNullOrWhiteSpace(storageDirectoryRelativePath))
            {
                storageDirectoryRelativePath = storageDirectoryRelativePath.Trim();

                storageDirectoryRelativePath = storageDirectoryRelativePath.StartsWith(@"/") || storageDirectoryRelativePath.StartsWith(@"\")
                    ? storageDirectoryRelativePath.Substring(1)
                    : storageDirectoryRelativePath;

                storageDirectoryRelativePath = storageDirectoryRelativePath.Replace(@"\", @"/");
            }


            var storageDirectoryInfo =
                string.IsNullOrWhiteSpace(KoalaGlobals.AppIdentifier)
                    ? string.IsNullOrWhiteSpace(storageDirectoryRelativePath)
                        ? (useDefaultDirectory ?? true)
                            ? new DirectoryInfo(Path.Combine(rootDirectoryInfo.FullName, defaultDirectoryName))
                            : rootDirectoryInfo
                        : (useDefaultDirectory ?? true)
                            ? new DirectoryInfo(Path.Combine(rootDirectoryInfo.FullName, defaultDirectoryName, storageDirectoryRelativePath))
                            : new DirectoryInfo(Path.Combine(rootDirectoryInfo.FullName, storageDirectoryRelativePath))
                    : string.IsNullOrWhiteSpace(storageDirectoryRelativePath)
                        ? (useDefaultDirectory ?? true)
                            ? new DirectoryInfo(Path.Combine(rootDirectoryInfo.FullName, KoalaGlobals.AppIdentifier.Trim(), defaultDirectoryName))
                            : new DirectoryInfo(Path.Combine(rootDirectoryInfo.FullName, KoalaGlobals.AppIdentifier.Trim()))
                        : (useDefaultDirectory ?? true)
                            ? new DirectoryInfo(Path.Combine(rootDirectoryInfo.FullName, KoalaGlobals.AppIdentifier.Trim(), defaultDirectoryName, storageDirectoryRelativePath))
                            : new DirectoryInfo(Path.Combine(rootDirectoryInfo.FullName, KoalaGlobals.AppIdentifier.Trim(), storageDirectoryRelativePath))
                ;

            if (!storageDirectoryInfo.Exists)
            {
                Directory.CreateDirectory(storageDirectoryInfo.FullName);
            }

            return storageDirectoryInfo;
        }

        private static readonly object GetFromFileLock = new object();
        private static readonly object GetFileBinaryLock = new object();
        private static readonly object GetFileStringLock = new object();
        private static readonly object GetFileStreamLock = new object();
    }
}