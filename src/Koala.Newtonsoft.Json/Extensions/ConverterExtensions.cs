using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.SmartEnums;

namespace Newtonsoft.Json.Extensions
{
    public static class ConverterExtensions
    {
        public static bool IsA(this Type type, Type typeToBe)
        {
            if (!typeToBe.IsGenericTypeDefinition)
                return typeToBe.IsAssignableFrom(type);

            var toCheckTypes = new List<Type> { type };
            if (typeToBe.IsInterface)
                toCheckTypes.AddRange(type.GetInterfaces());

            var basedOn = type;
            while (basedOn.BaseType != null)
            {
                toCheckTypes.Add(basedOn.BaseType);
                basedOn = basedOn.BaseType;
            }

            return toCheckTypes.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeToBe);
        }

        public static T ToSmartEnum<T>(this string value, T defaultValue = default) where T : AbstractSmartEnum
        {
            if (value.IsFullSmartEnumString())
            {
                var targetObject = value.GetAbstractSmartEnumObject();

                if (targetObject == null) return defaultValue;

                return (T)targetObject;
            }

            return value.GetAbstractSmartEnumObject<T>(defaultValue);
        }

        public static object GetAbstractSmartEnumObject(this string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains("AbstractSmartEnum"))
            {
                return null;
            }

            var valuePartFirst = value.Split(new[] { "[" }, StringSplitOptions.RemoveEmptyEntries)
                .First()
                .Replace(",", string.Empty).Replace("]", string.Empty).Replace("[", string.Empty)
                .Trim();

            var objectTypeName = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Last()
                .Replace(",", string.Empty).Replace("]", string.Empty).Replace("[", string.Empty)
                .Trim();

            var objectType = objectTypeName.FindType();

            var valueFieldInfo = objectType.GetField(valuePartFirst, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField);

            if (valueFieldInfo == null)
            {
                return default;
            }

            return valueFieldInfo.GetValue(null);
        }

        public static T GetAbstractSmartEnumObject<T>(this string value, T defaultValue = default)
        {
            var objectType = typeof(T);

            var valueFieldInfo = objectType.GetField(value, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField);

            if (valueFieldInfo == null)
            {
                return default;
            }

            return (T)valueFieldInfo.GetValue(null);
        }

        public static Type FindType(this string typeFullName)
        {
            // Debug.Assert(typeFullName != null);

            if (!string.IsNullOrWhiteSpace(typeFullName))
            {
                if (ResolvedTypes.TryGetValue(typeFullName, out var t))
                {
                    return t;
                }

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    // Console.WriteLine($@"{typeFullName}: {assembly.FullName}");

                    t = assembly.GetType(typeFullName, false);

                    if (t != null)
                    {
                        ResolvedTypes[typeFullName] = t;
                        return t;
                    }
                }
            }

            throw new ArgumentException($"Type provided [{typeFullName}] doesn't exist or invalid in the current app domain");
        }

        public static IDictionary<string, Type> ResolvedTypes = new ConcurrentDictionary<string, Type>();

        private static bool IsFullSmartEnumString(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Contains("AbstractSmartEnum") && value.Contains("[") && value.Contains("]") && value.Contains(",");
        }
    }
}
