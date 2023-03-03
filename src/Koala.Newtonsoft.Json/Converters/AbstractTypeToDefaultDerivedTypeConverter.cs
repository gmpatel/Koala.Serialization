using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;

namespace Newtonsoft.Json.Converters
{
    public class AbstractTypeToDefaultDerivedTypeConverter : JsonConverter
    {
        private readonly ILogger logger = Log.ForContext(typeof(AbstractTypeToDefaultDerivedTypeConverter));

        public readonly IDictionary<string, string> AbstractTypeToDefaultDerivedTypeMappings =
            new Dictionary<string, string>
            {
                { "ArchiSharp.Compliance.Core.Models.Elements.Site", "ArchiSharp.Compliance.Core.Models.Elements.SiteLegacy, ArchiSharp.Compliance.Core" },
                { "ArchiSharp.Compliance.Core.Models.Elements.Lot", "ArchiSharp.Compliance.Core.Models.Elements.LotLegacy, ArchiSharp.Compliance.Core" },
                { "ArchiSharp.Compliance.Core.Models.Elements.House", "ArchiSharp.Compliance.Core.Models.Elements.HouseLegacy, ArchiSharp.Compliance.Core" },

                { "ArchiSharp.ECheck.Core.Abstraction.Models.Elements.Site", "ArchiSharp.Compliance.Core.Models.Elements.SiteLegacy, ArchiSharp.Compliance.Core" },
                { "ArchiSharp.ECheck.Core.Abstraction.Models.Elements.Lot", "ArchiSharp.Compliance.Core.Models.Elements.LotLegacy, ArchiSharp.Compliance.Core" },
                { "ArchiSharp.ECheck.Core.Abstraction.Models.Elements.House", "ArchiSharp.Compliance.Core.Models.Elements.HouseLegacy, ArchiSharp.Compliance.Core" },
            };

        public override bool CanConvert(Type objectType)
        {
            var objectTypeFullName = objectType.FullName;
            var result = !string.IsNullOrWhiteSpace(objectTypeFullName) && this.AbstractTypeToDefaultDerivedTypeMappings.ContainsKey(objectTypeFullName);

            logger.Debug($"AbstractTypeToDefaultDerivedTypeConverter, CanConvert(), objectType = {objectTypeFullName}, objectTypeInDict = {this.AbstractTypeToDefaultDerivedTypeMappings.ContainsKey(objectTypeFullName)}, result = {result}");

            return result;
        }

        public override bool CanRead => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            if (!jObject.TryGetValue("$type", StringComparison.InvariantCultureIgnoreCase, out var objectDerivedType))
            {
                var objectDefaultDerivedType = !string.IsNullOrWhiteSpace(objectType.FullName)
                    ? this.AbstractTypeToDefaultDerivedTypeMappings[objectType.FullName]
                    : objectType.FullName;

                jObject["$type"] = objectDefaultDerivedType;

                objectDerivedType = new JValue(objectDefaultDerivedType);
            }

            var objectDerivedTypeFullName = objectDerivedType.ToString();
            var json = jObject.ToString();
            var targetType = Type.GetType(objectDerivedTypeFullName) ?? objectType;


            var methodInfo = typeof(NewtonsoftJsonSerializationExtensions).GetMethod("Get",
                new[] { typeof(string), typeof(bool?), typeof(bool?) });

            var genericMethod = methodInfo.MakeGenericMethod(targetType);

            var objectConverted = genericMethod.Invoke(null, new object[] { json, true, true });

            logger.Debug($"AbstractTypeToDefaultDerivedTypeConverter, ReadJson(), objectDerivedTypeFullName = {objectDerivedTypeFullName}, json = {json}, targetType = {targetType.FullName}");

            return objectConverted;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}