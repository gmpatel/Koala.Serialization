using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Properties;
using Serilog.ThrowContext;
using Serilog.Formatting.Json;
using Serilog.Formatting;

namespace Koala.Core
{
    public enum HttpClientMethods
    {
        GET,
        HEAD,
        POST,
        PUT,
        DELETE,
        CONNECT,
        OPTIONS,
        TRACE,
        PATCH,
    }

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
        public static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            return string.IsNullOrWhiteSpace(value) || !Enum.TryParse(value.Trim(), true, out T result)
                ? defaultValue
                : result;
        }

        public static string Short(this Guid guid)
        {
            return guid.ShortGuid().Replace("=", string.Empty).Trim();
        }

        public static string ShortGuid(this Guid guid)
        {
            return Convert.ToBase64String(guid.ToByteArray());
        }

        public static bool IsHttpUrl(this Uri url)
        {
            return (url == null ? string.Empty : url.ToString()).IsHttpUrl();
        }
        
        public static bool IsHttpUrl(this string url)
        {
            if (!string.IsNullOrWhiteSpace(url) && (url.Trim().StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) || url.Trim().StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        public static string GetFileExtensionFromS3Url(this string fileUrl, bool includeDot = false)
        {
            if (fileUrl.IsHttpUrl())
            {
                var ext = fileUrl.Split('?')[0];
                ext = ext.Split('/').Last();
                ext = ext.Contains('.') ? ext.Substring(ext.LastIndexOf('.')).Trim().ToLower() : string.Empty;
                return includeDot ? ext : ext.Replace(".", string.Empty);
            }
            else
            {
                var ext = Path.GetExtension(fileUrl).Trim().ToLower();
                return includeDot ? ext : ext.Replace(".", string.Empty);
            }
        }
        
        public static Logger GetLogger(this object input, IConfiguration configuration)
        {
            return input.GetLogger(configuration, out var loggingLevelSwitch, out var loggerConfiguration);
        }

        public static Logger GetLogger(this object input, IConfiguration configuration, out LoggingLevelSwitch loggingLevelSwitch)
        {
            return input.GetLogger(configuration, out loggingLevelSwitch, out var loggerConfiguration);
        }

        public static Logger GetLogger(this object input, IConfiguration configuration, out LoggingLevelSwitch loggingLevelSwitch, out LoggerConfiguration loggerConfiguration)
        {
            var path = "logs-.log".GetStorageFile("app-logs", false).DirectoryName ?? "/tmp/app-logs/logs-.log";
            var jsonFormatter = new JsonFormatter(renderMessage: true);
            
            loggingLevelSwitch = input.GetDefaultLoggingLevelSwitch();

            loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(loggingLevelSwitch)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Extensions.Http.DefaultHttpClientFactory", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.With<ThrowContextEnricher>()
                .Enrich.With(new ExceptionEnricher())
                .Enrich.With(new MessageEnricher())
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: Resources.LogConsoleLogsOutputTemplateEnricher, theme: AnsiConsoleTheme.Literate)
                .WriteTo.File(
                    path, 
                    rollingInterval: RollingInterval.Hour, 
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 123456,
                    shared: true
                );

            if (configuration != null)
            {
                loggerConfiguration = loggerConfiguration.ReadFrom.Configuration(configuration);
            }
                
            var logger = loggerConfiguration.CreateLogger();

            Log.Logger = logger;

            return logger;
        }

        public static LoggingLevelSwitch GetDefaultLoggingLevelSwitch(this object input)
        {
            return new LoggingLevelSwitch
            {
                MinimumLevel = "LOG_EVENT_LEVEL".GetEnvVarValue().ToEnum<LogEventLevel>(input.IsDebugMode() ? LogEventLevel.Debug : LogEventLevel.Information)
            };
        }

        public static bool IsFileUrlOfType(this string fileUrl, out string fileExtension, params string[] extensions)
        {
            var fileExtensionInt = fileUrl.GetFileExtensionFromS3Url();
            fileExtension = fileExtensionInt;
            return extensions.Any(x => x.Trim().ToLower().Equals(string.IsNullOrWhiteSpace(fileExtensionInt) ? string.Empty : $".{fileExtensionInt}"));
        }

        public static bool IsWindows(this object input) =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS(this object input) =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux(this object input) =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

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

        public static string GetEnvVarValue(this object input, string variableName = default, string defaultVariableValue = default)
        {
            string variableValue;

            if (string.IsNullOrEmpty(variableValue = Environment.GetEnvironmentVariable(variableName ?? input.ToString(), EnvironmentVariableTarget.Process)))
            {
                if (string.IsNullOrEmpty(variableValue = Environment.GetEnvironmentVariable(variableName ?? input.ToString(), EnvironmentVariableTarget.User)))
                {
                    if (string.IsNullOrEmpty(variableValue = Environment.GetEnvironmentVariable(variableName ?? input.ToString(), EnvironmentVariableTarget.Machine)))
                    {
                        return defaultVariableValue;
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

    public class ErrorResponse
    {
        public string Status { get; private set; }

        public string Message { get; private set; }

        public string Type { get; private set; }

        public dynamic DataToken { get; private set; }

        public dynamic DataTokens { get; private set; }

        public string StackTrace { get; private set; }

        public ErrorResponse(Exception exception, HttpStatusCode status = HttpStatusCode.InternalServerError, bool? stackTrace = default)
        {
            Status = $"{status} ({(int)status})";
            Message = exception?.Message;
            Type = $"{exception?.GetType().Name}";

            var propertyDataToken = exception.GetType().GetProperty(nameof(DataToken));
            this.DataToken = propertyDataToken?.GetValue(exception);

            var propertyDataTokens = exception.GetType().GetProperty(nameof(DataTokens));
            this.DataTokens = propertyDataTokens?.GetValue(exception);

            StackTrace = StackTraceOverride ?? stackTrace ?? this.IsDebugMode()
                ? exception?.ToString()
                : null;
        }

        public static bool? StackTraceOverride { get; set; }
    }

    public class ExceptionEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent?.Exception == null)
                return;

            var escapedException = Regex.Replace(logEvent.Exception.ToString(), Environment.NewLine, "[BR]");
            escapedException = Regex.Replace(escapedException, "\r\n", "[BR]");
            escapedException = Regex.Replace(escapedException, "\n", "[BR]");
            escapedException = Regex.Replace(escapedException, "\r", "[BR]");

            var logEventProperty = propertyFactory.CreateProperty("EscapedException", escapedException);
            logEvent.AddPropertyIfAbsent(logEventProperty);

            if (logEvent.Exception.Data.Count == 0)
            {
                return;
            }

            var dataDictionary = logEvent.Exception.Data
                .Cast<DictionaryEntry>()
                .Where(e => e.Key is string)
                .ToDictionary(e => (string)e.Key, e => e.Value);

            var property = propertyFactory.CreateProperty("ExceptionData", dataDictionary, destructureObjects: true);

            logEvent.AddPropertyIfAbsent(property);
        }
    }

    public class MessageEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.MessageTemplate == null)
                return;

            var escapedMessage = Regex.Replace(logEvent.MessageTemplate.ToString(), Environment.NewLine, "[BR]");
            escapedMessage = Regex.Replace(escapedMessage, "\r\n", "[BR]");
            escapedMessage = Regex.Replace(escapedMessage, "\n", "[BR]");
            escapedMessage = Regex.Replace(escapedMessage, "\r", "[BR]");

            var logEventProperty = propertyFactory.CreateProperty("EscapedMessage", escapedMessage);
            logEvent.AddPropertyIfAbsent(logEventProperty);
        }
    }
}
