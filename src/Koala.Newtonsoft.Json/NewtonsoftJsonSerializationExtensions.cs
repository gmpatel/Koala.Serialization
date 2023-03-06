using System;
using Newtonsoft.Json.Properties;
using Newtonsoft.Json.Encryption;
using Newtonsoft.Json.DataExtensions;

namespace Newtonsoft.Json
{
    public static class NewtonsoftJsonSerializationExtensions
    {
        public static JsonSerializerSettings GetOrMergeDefaultJsonSerializerSettings(this JsonSerializerSettings serializerSettings)
        {
            var settings = NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonSettings;

            if (serializerSettings == null)
            {
                return (serializerSettings = settings);
            }

            serializerSettings.ContractResolver = settings.ContractResolver;
            serializerSettings.Converters = settings.Converters;
            serializerSettings.NullValueHandling = settings.NullValueHandling;
            serializerSettings.DefaultValueHandling = settings.DefaultValueHandling;
            serializerSettings.ReferenceLoopHandling = settings.ReferenceLoopHandling;
            serializerSettings.Formatting = settings.Formatting;
            serializerSettings.DateFormatHandling = settings.DateFormatHandling;
            serializerSettings.ConstructorHandling = settings.ConstructorHandling;
            serializerSettings.TypeNameHandling = settings.TypeNameHandling;
            serializerSettings.SerializationBinder = settings.SerializationBinder;

            return serializerSettings;
        }

        public static JsonSerializerSettings GetOrMergeDotNetFwNewtonsoftJsonNonIndentedSettings(this JsonSerializerSettings settings)
        {
            if (settings == null)
            {
                settings = NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonNonIndentedSettings;
            }

            settings.SerializationBinder = new CustomTypeConversionBinder();

            return settings;
        }

        public static T JsonClone<T>(this T input, bool applySettings = true, bool? format = default)
        {
            var settings = (format ?? true)
                ? NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonSettings
                : NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonNonIndentedSettings;

            var json = applySettings
                ? JsonConvert.SerializeObject(input, settings)
                : JsonConvert.SerializeObject(input);

            return applySettings
                ? JsonConvert.DeserializeObject<T>(json, NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonSettings)
                : JsonConvert.DeserializeObject<T>(json);
        }

        public static TOut JsonClone<TIn, TOut>(this TIn input, bool applySettings = true, bool? format = default)
        {
            var settings = (format ?? true)
                ? NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonSettings
                : NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonNonIndentedSettings;

            var json = applySettings
                ? JsonConvert.SerializeObject(input, settings)
                : JsonConvert.SerializeObject(input);

            return applySettings
                ? JsonConvert.DeserializeObject<TOut>(json, NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonSettings)
                : JsonConvert.DeserializeObject<TOut>(json);
        }

        public static T Get<T>(this object input, bool? applySettings = default, bool? dotNetCore = default)
        {
            var json = input?.ToString();

            var result = (string.IsNullOrWhiteSpace(json) || json.Trim().Equals(Resources.StringValueAsNull, StringComparison.InvariantCultureIgnoreCase))
                ? default(T)
                : json.Equals(Resources.StringValueAsEmptyJson)
                    ? default(T)
                    : json.Get<T>(applySettings, dotNetCore);

            return result;
        }

        public static T Get<T>(this string input, bool? applySettings = default, bool? dotNetCore = default)
        {
            var result = (string.IsNullOrWhiteSpace(input))
                ? default(T)
                : (applySettings ?? true)
                    ? (dotNetCore ?? true)
                        ? Get<T>(input, NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonSettings)
                        : Get<T>(input, NewtonsoftJsonSerializationSettings.DotNetFwNewtonsoftJsonNonIndentedSettings)
                    : Get<T>(input, settings: null);

            return result;
        }

        public static T Get<T>(this string input, JsonSerializerSettings settings, bool? dotNetCore = default)
        {
            var result = (string.IsNullOrWhiteSpace(input))
                ? default(T)
                : (settings != null)
                    ? (dotNetCore ?? true)
                        ? JsonConvert.DeserializeObject<T>(input, settings)
                        : JsonConvert.DeserializeObject<T>(input, settings.GetOrMergeDotNetFwNewtonsoftJsonNonIndentedSettings())
                    : JsonConvert.DeserializeObject<T>(input);

            return result;
        }

        public static string Json<T>(this T input, JsonSerializerSettings settings)
        {
            var json = settings != null
                ? JsonConvert.SerializeObject(input, settings)
                : JsonConvert.SerializeObject(input);

            return json;
        }

        public static string Json<T>(this T input, bool applySettings = true, bool? format = default, bool? includeAllProperties = default, bool? compact = default, bool? custom = default)
        {
            var settings = (custom ?? false)
                ? NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftCustomSettings
                : applySettings
                    ? (format ?? true)
                        ? (includeAllProperties ?? false)
                            ? NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonIncludeAllPropertiesIndentedSettings
                            : (compact ?? false)
                                ? NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftCompactIndentedJsonSettings
                                : NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonSettings
                        : (includeAllProperties ?? false)
                            ? NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonIncludeAllPropertiesNonIndentedSettings
                            : (compact ?? false)
                                ? NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftCompactJsonSettings
                                : NewtonsoftJsonSerializationSettings.ApplicationNewtonsoftJsonNonIndentedSettings
                    : null;

            var json = input.Json(settings);

            return json;
        }

        public static string Compress<T>(this T input, bool applySettings = true, bool? format = false)
        {
            var jsonString = input.Json(applySettings, format);
            var jsonStringZippedBytes = jsonString.Zip();
            var jsonStringZippedBytesBase64String = Convert.ToBase64String(jsonStringZippedBytes);
            return jsonStringZippedBytesBase64String;
        }

        public static T Uncompress<T>(this object input, bool? applySettings = default, bool? dotNetCore = default)
        {
            var json = input?.ToString();

            var result = (string.IsNullOrWhiteSpace(json) || json.Trim().Equals(Resources.StringValueAsNull, StringComparison.InvariantCultureIgnoreCase))
                ? default(T)
                : json.Equals(Resources.StringValueAsEmptyJson)
                    ? default(T)
                    : json.Uncompress<T>(applySettings, dotNetCore);

            return result;
        }

        public static T Uncompress<T>(this string input, bool? applySettings = default, bool? dotNetCore = default)
        {
            var jsonStringZippedBytesBack = Convert.FromBase64String(input);
            var jsonStringBack = jsonStringZippedBytesBack.Unzip();
            var output = jsonStringBack.Get<T>(applySettings, dotNetCore);
            return output;
        }

        public static byte[] Encrypt<T>(this T input, bool applySettings = true, bool? format = false)
        {
            var jsonString = input.Json(applySettings, format);
            var jsonStringZippedBytes = jsonString.Zip();
            return Cryptography.Encrypt(jsonStringZippedBytes);
        }

        public static T Decrypt<T>(this byte[] input, bool? applySettings = default, bool? dotNetCore = default)
        {
            var decryptedBytes = input.Decrypt();
            var jsonStringBack = decryptedBytes.Unzip();
            var output = jsonStringBack.Get<T>(applySettings, dotNetCore);
            return output;
        }
    }
}