using System;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using System.Linq;
using UnitsNet.Serialization.JsonNet;

namespace Newtonsoft.Json
{
    public static class NewtonsoftJsonSerializationSettings
    {
        private static JsonSerializerSettings jsonApplicationSettings { get; set; }

        public static JsonSerializerSettings ApplicationNewtonsoftJsonSettings
        {
            get
            {
                if (jsonApplicationSettings == null)
                {
                    jsonApplicationSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCaseExceptDictionaryKeysResolver(),
                        Converters = new List<JsonConverter>
                        {
                            new StringEnumConverter { AllowIntegerValues = true },
                            new UnitsNetIQuantityJsonConverter(),
                            new SmartEnumConvertor(), // new SmartEnumDictionaryConvertor()
                        },
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Formatting = Formatting.Indented,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        ConstructorHandling = ConstructorHandling.Default,
                        TypeNameHandling = TypeNameHandling.Auto
                    };
                }

                return jsonApplicationSettings;
            }
        }

        private static JsonSerializerSettings jsonCompactIndentedSettings { get; set; }

        public static JsonSerializerSettings ApplicationNewtonsoftCompactIndentedJsonSettings
        {
            get
            {
                if (jsonCompactIndentedSettings == null)
                {
                    jsonCompactIndentedSettings = ApplicationNewtonsoftJsonSettings.JsonClone();
                    jsonCompactIndentedSettings.TypeNameHandling = TypeNameHandling.None;
                }

                return jsonCompactIndentedSettings;
            }
        }

        private static JsonSerializerSettings jsonCompactSettings { get; set; }

        public static JsonSerializerSettings ApplicationNewtonsoftCompactJsonSettings
        {
            get
            {
                if (jsonCompactSettings == null)
                {
                    jsonCompactSettings = ApplicationNewtonsoftJsonSettings.JsonClone();
                    jsonCompactSettings.TypeNameHandling = TypeNameHandling.None;
                    jsonCompactSettings.Formatting = Formatting.None;
                }

                return jsonCompactSettings;
            }
        }

        private static JsonSerializerSettings jsonNotIndentedSettings { get; set; }

        public static JsonSerializerSettings ApplicationNewtonsoftJsonNonIndentedSettings
        {
            get
            {
                if (jsonNotIndentedSettings == null)
                {
                    jsonNotIndentedSettings = ApplicationNewtonsoftJsonSettings.JsonClone();
                    jsonNotIndentedSettings.Formatting = Formatting.None;
                }

                return jsonNotIndentedSettings;
            }
        }

        private static JsonSerializerSettings jsonIncludeAllPropertiesNonIndentedSettings { get; set; }

        public static JsonSerializerSettings ApplicationNewtonsoftJsonIncludeAllPropertiesNonIndentedSettings
        {
            get
            {
                if (jsonIncludeAllPropertiesNonIndentedSettings == null)
                {
                    jsonIncludeAllPropertiesNonIndentedSettings = ApplicationNewtonsoftJsonSettings.JsonClone();
                    jsonIncludeAllPropertiesNonIndentedSettings.Formatting = Formatting.None;
                    jsonIncludeAllPropertiesNonIndentedSettings.NullValueHandling = NullValueHandling.Include;
                    jsonIncludeAllPropertiesNonIndentedSettings.DefaultValueHandling = DefaultValueHandling.Include;
                }

                return jsonIncludeAllPropertiesNonIndentedSettings;
            }
        }

        private static JsonSerializerSettings jsonIncludeAllPropertiesIndentedSettings { get; set; }

        public static JsonSerializerSettings ApplicationNewtonsoftJsonIncludeAllPropertiesIndentedSettings
        {
            get
            {
                if (jsonIncludeAllPropertiesIndentedSettings == null)
                {
                    jsonIncludeAllPropertiesIndentedSettings = ApplicationNewtonsoftJsonSettings.JsonClone();
                    jsonIncludeAllPropertiesIndentedSettings.NullValueHandling = NullValueHandling.Include;
                    jsonIncludeAllPropertiesIndentedSettings.DefaultValueHandling = DefaultValueHandling.Include;
                }

                return jsonIncludeAllPropertiesIndentedSettings;
            }
        }

        private static JsonSerializerSettings jsonDotNetFwNotIndentedSettings { get; set; }

        public static JsonSerializerSettings DotNetFwNewtonsoftJsonNonIndentedSettings
        {
            get
            {
                if (jsonDotNetFwNotIndentedSettings == null)
                {
                    jsonDotNetFwNotIndentedSettings = ApplicationNewtonsoftJsonSettings.JsonClone();
                    jsonDotNetFwNotIndentedSettings.Formatting = Formatting.None;
                    jsonDotNetFwNotIndentedSettings.SerializationBinder = new CustomTypeConversionBinder();
                }

                return jsonDotNetFwNotIndentedSettings;
            }
        }

        private static JsonSerializerSettings jsonCustomSettings { get; set; }

        public static JsonSerializerSettings ApplicationNewtonsoftCustomSettings
        {
            get
            {
                if (jsonCustomSettings == null)
                {
                    jsonCustomSettings = ApplicationNewtonsoftJsonSettings.JsonClone();
                }

                return jsonCustomSettings;
            }
            set
            {
                jsonCustomSettings = value;
            }
        }
    }

    public class CustomTypeConversionBinder : DefaultSerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            try
            {
                return base.BindToType(assemblyName, typeName);
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(assemblyName) && assemblyName.Equals("System.Private.CoreLib", StringComparison.CurrentCultureIgnoreCase))
                {
                    assemblyName = assemblyName.Replace("System.Private.CoreLib", "mscorlib");
                    typeName = typeName.Replace("System.Private.CoreLib", "mscorlib");
                }

                return base.BindToType(assemblyName, typeName);
            }
        }
    }

    public class CamelCaseExceptDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            var contract = base.CreateDictionaryContract(objectType);

            contract.DictionaryKeyResolver = propertyName => propertyName;

            return contract;
        }
    }
}