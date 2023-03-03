using System;
using Newtonsoft.Json.Extensions;
using Newtonsoft.Json.SmartEnums;

namespace Newtonsoft.Json.Converters
{
    public class SmartEnumConvertor : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var stringValue = reader.Value?.ToString();

            if (string.IsNullOrWhiteSpace(stringValue) || !stringValue.Contains($"[{nameof(AbstractSmartEnum)},"))
            {
                return reader.Value;
            }

            return stringValue.GetAbstractSmartEnumObject();
        }

        public override bool CanConvert(Type objectType)
        {
            var result = typeof(AbstractSmartEnum).IsAssignableFrom(objectType);
            return result;
        }
    }
}