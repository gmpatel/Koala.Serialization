using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Koala.Core
{
    public enum AppBuilds
    {
        DEBUG,
        RELEASE
    }

    public static class MiscExtensions
    {
        public static bool IsDebugMode(this object input, Assembly assembly = null)
        {
            return input.IsAppDebugBuild(assembly);

            // #if DEBUG
            //     return true;
            // #else
            //     return false;
            // #endif
        }

        public static bool IsReleaseMode(this object input, Assembly assembly = null)
        {
            return !input.IsAppDebugBuild(assembly);

            // #if RELEASE
            //     return true;
            // #else
            //     return false;
            // #endif
        }

        public static string CompileModeLabel(this object input, Assembly assembly = null)
        {
            return input.IsReleaseMode(assembly) ? nameof(AppBuilds.RELEASE) : nameof(AppBuilds.DEBUG);
        }

        public static AppBuilds AppBuild(this object input, Assembly assembly = null)
        {
            return input.IsAppDebugBuild() ? AppBuilds.DEBUG : AppBuilds.RELEASE;
        }

        private static bool IsAppDebugBuild(this object input, Assembly assembly = null)
        {
            var operatingAssembly = assembly ?? Assembly.GetEntryAssembly();

            if (operatingAssembly == null)
                throw new ArgumentNullException(nameof(assembly), @"Either assembly not supplied or the entry level assembly not found!");

            return operatingAssembly.GetCustomAttributes(false)
                .OfType<DebuggableAttribute>()
                .Any(da => da.IsJITTrackingEnabled);
        }

        public static string GetEnvVarValue(this object input, string variableName = null)
        {
            string variableValue;

            if (string.IsNullOrEmpty(variableValue = Environment.GetEnvironmentVariable(variableName ?? input.ToString(), EnvironmentVariableTarget.Process)))
            {
                if (string.IsNullOrEmpty(variableValue = Environment.GetEnvironmentVariable(variableName ?? input.ToString(), EnvironmentVariableTarget.User)))
                {
                    if (string.IsNullOrEmpty(variableValue = Environment.GetEnvironmentVariable(variableName ?? input.ToString(), EnvironmentVariableTarget.Machine)))
                    {
                        return null;
                    }
                }
            }

            return variableValue;
        }

        public static string SetEnvVarValue(this string variableName, string variableValue, EnvironmentVariableTarget environmentTarget = EnvironmentVariableTarget.Process)
        {
            Environment.SetEnvironmentVariable(variableName, variableValue, environmentTarget);
            return variableValue;
        }

        public static T GetValueAs<T>(this IDictionary<string, object> data, string key)
        {
            if (data != null && data.TryGetValue(key, out var value))
            {
                return value.GetValueAs<T>();
            }

            return default;
        }

        public static T GetValueAs<T>(this object value)
        {
            try
            {
                var valueToConvert = value.ToString();
                return (T)Convert.ChangeType(valueToConvert, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        public static IList<TR> ForEach<T, TR>(this IList<T> items, Func<T, TR> func, bool throwIfInputNull = false)
        {
            if (items == null || func == null)
            {
                if (throwIfInputNull)
                {
                    throw new ArgumentNullException(items == null ? nameof(items) : nameof(func));
                }

                return null;
            }

            var returnList = new List<TR>();

            foreach (var item in items)
            {
                var returnListObject = func(item);
                returnList.Add(returnListObject);
            }

            return returnList;
        }

        public static List<TR> ForEach<T, TR>(this List<T> items, Func<T, TR> func, bool throwIfInputNull = false)
        {
            if (items == null || func == null)
            {
                if (throwIfInputNull)
                {
                    throw new ArgumentNullException(items == null ? nameof(items) : nameof(func));
                }

                return null;
            }

            var returnList = new List<TR>();

            foreach (var item in items)
            {
                var returnListObject = func(item);
                returnList.Add(returnListObject);
            }

            return returnList;
        }

        public static List<T> ForEachNew<T>(this List<T> items, Action<T> action, bool throwIfInputNull = false)
        {
            if (items == null || action == null)
            {
                if (throwIfInputNull)
                {
                    throw new ArgumentNullException(items == null ? nameof(items) : nameof(action));
                }

                return items;
            }

            foreach (var item in items)
            {
                action(item);
            }

            return items;
        }

        public static IList<T> ForEach<T>(this IList<T> items, Action<T> action, bool throwIfInputNull = false)
        {
            if (items == null || action == null)
            {
                if (throwIfInputNull)
                {
                    throw new ArgumentNullException(items == null ? nameof(items) : nameof(action));
                }

                return items;
            }

            foreach (var item in items)
            {
                action(item);
            }

            return items;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> action, bool throwIfInputNull = false)
        {
            if (items == null || action == null)
            {
                if (throwIfInputNull)
                {
                    throw new ArgumentNullException(items == null ? nameof(items) : nameof(action));
                }

                return items;
            }

            var list = items as T[] ?? items.ToArray();

            foreach (var item in list)
            {
                action(item);
            }

            return list;
        }

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            if (list == null || items == null)
            {
                throw new ArgumentNullException(list == null ? nameof(list) : nameof(items));
            }

            if (list is List<T> asList)
            {
                asList.AddRange(items);
            }
            else
            {
                foreach (var item in items)
                {
                    list.Add(item);
                }
            }
        }

        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            foreach (var element in source)
                target.Add(element);
        }

        public static IList<T> RemoveAll<T>(this IList<T> items, Predicate<T> action)
        {
            if (items == null || action == null)
            {
                throw new ArgumentNullException(items == null ? nameof(items) : nameof(action));
            }

            var returnItems = new List<T>();
            foreach (var item in items)
            {
                if (!action(item))
                    returnItems.Add(item);
            }

            return returnItems;
        }

        public static string GetContainerName(this object input)
        {
            return ContainerName;
        }

        public static IEnumerable<string> GetContainerIPs(this object input)
        {
            return ContainerIPs;
        }

        public static string GetContainerIPsString(this object input)
        {
            return ContainerIPsString;
        }

        public static IEnumerable<string> GetHostNames(this object input)
        {
            return HostNames;
        }

        public static string GetHostNamesString(this object input)
        {
            return HostNamesString;
        }

        public static string GetVersion(this object input)
        {
            return Version;
        }

        static MiscExtensions()
        {
            try
            {
                Version = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString();
                ContainerName = Dns.GetHostName();
                ContainerIPs = Dns.GetHostEntry(ContainerName).AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).Select(x => x.ToString());
                ContainerIPsString = string.Join("; ", ContainerIPs);
                HostNames = new[] { ContainerName, $"{ Environment.MachineName } ({ Environment.OSVersion })" };
                HostNamesString = string.Join("; ", HostNames);
            }
            finally
            {
                // Do Nothing ...
            }
        }

        private static string Version { get; set; }

        private static string ContainerName { get; set; }

        private static IEnumerable<string> ContainerIPs { get; set; }

        private static string ContainerIPsString { get; set; }

        private static IEnumerable<string> HostNames { get; set; }

        private static string HostNamesString { get; set; }
    }
}
