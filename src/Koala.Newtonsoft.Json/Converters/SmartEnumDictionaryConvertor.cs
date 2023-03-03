using Newtonsoft.Json.SmartEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Extensions;

namespace Newtonsoft.Json.Converters
{
    public class SmartEnumDictionaryConvertor : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var valueType = objectType.GetGenericArguments()[1];
            var intermediateDictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);
            var intermediateDictionary = (IDictionary)Activator.CreateInstance(intermediateDictionaryType);
            var finalDictionary = (IDictionary)Activator.CreateInstance(objectType);

            serializer.Populate(reader, intermediateDictionary);

            foreach (DictionaryEntry pair in intermediateDictionary)
            {
                var key = (pair.Key as string).GetAbstractSmartEnumObject();
                var value = pair.Value;

                finalDictionary.Add(key, value);
            }


            return finalDictionary;
        }

        public override bool CanConvert(Type objectType)
        {
            if (!objectType.IsGenericType)
            {
                return false;
            }

            if (objectType.GetGenericTypeDefinition() != typeof(Dictionary<,>))
            {
                return false;
            }

            return typeof(AbstractSmartEnum).IsAssignableFrom(objectType.GetGenericArguments()[0]);
        }
    }
}