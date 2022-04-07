using System;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace Newtonsoft.Json
{
    public static class NewtonsoftJsonSerializationSettings
    {
        public static readonly IDictionary<string, JsonConverter> Converters;

        private static JsonSerializerSettings jsonDotNetFwNotIndentedSettings { get; set; }
        private static JsonSerializerSettings jsonIndentedSettings { get; set; }
        private static JsonSerializerSettings jsonNotIndentedSettings { get; set; }
        private static JsonSerializerSettings jsonIncludeAllPropertiesNonIndentedSettings { get; set; }
        private static JsonSerializerSettings jsonIncludeAllPropertiesIndentedSettings { get; set; }

        static NewtonsoftJsonSerializationSettings()
        {
            Converters = new Dictionary<string, JsonConverter> 
            {
                { typeof(StringEnumConverter).Name, new StringEnumConverter { AllowIntegerValues = true } }
            };
        }

        public static void RegistreJsonConverter(this JsonConverter converter)
        {
            Converters[converter.GetType().Name] = converter;
            jsonIndentedSettings = null;
        }

        public static JsonSerializerSettings ApplicationNewtonsoftJsonSettings
        {
            get
            {
                if (jsonIndentedSettings == null)
                {
                    jsonIndentedSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCaseExceptDictionaryKeysResolver(),
                        Converters = Converters.Select(x => x.Value).ToList(),
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Formatting = Formatting.Indented,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        ConstructorHandling = ConstructorHandling.Default,
                        TypeNameHandling = TypeNameHandling.Auto          
                    };
                }

                return jsonIndentedSettings;
            }
        }

        public static JsonSerializerSettings ApplicationNewtonsoftJsonNonIndentedSettings
        {
            get
            {
                if (jsonNotIndentedSettings == null)
                {
                    jsonNotIndentedSettings = ApplicationNewtonsoftJsonSettings;
                    jsonNotIndentedSettings.Formatting = Formatting.None;
                }

                return jsonNotIndentedSettings;
            }
        }

        public static JsonSerializerSettings ApplicationNewtonsoftJsonIncludeAllPropertiesNonIndentedSettings
        {
            get
            {
                if (jsonIncludeAllPropertiesNonIndentedSettings == null)
                {
                    jsonIncludeAllPropertiesNonIndentedSettings = ApplicationNewtonsoftJsonSettings;
                    jsonIncludeAllPropertiesNonIndentedSettings.Formatting = Formatting.None;
                    jsonIncludeAllPropertiesNonIndentedSettings.NullValueHandling = NullValueHandling.Include;
                    jsonIncludeAllPropertiesNonIndentedSettings.DefaultValueHandling = DefaultValueHandling.Include;
                }

                return jsonIncludeAllPropertiesNonIndentedSettings;
            }
        }

        public static JsonSerializerSettings ApplicationNewtonsoftJsonIncludeAllPropertiesIndentedSettings
        {
            get
            {
                if (jsonIncludeAllPropertiesIndentedSettings == null)
                {
                    jsonIncludeAllPropertiesIndentedSettings = ApplicationNewtonsoftJsonSettings;
                    jsonIncludeAllPropertiesIndentedSettings.Formatting = Formatting.Indented;
                    jsonIncludeAllPropertiesIndentedSettings.NullValueHandling = NullValueHandling.Include;
                    jsonIncludeAllPropertiesIndentedSettings.DefaultValueHandling = DefaultValueHandling.Include;
                }

                return jsonIncludeAllPropertiesIndentedSettings;
            }
        }

        public static JsonSerializerSettings DotNetFwNewtonsoftJsonNonIndentedSettings
        {
            get
            {
                if (jsonDotNetFwNotIndentedSettings == null)
                {
                    jsonDotNetFwNotIndentedSettings = ApplicationNewtonsoftJsonNonIndentedSettings;
                    jsonDotNetFwNotIndentedSettings.SerializationBinder = new CustomTypeConversionBinder();
                }

                return jsonDotNetFwNotIndentedSettings;
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