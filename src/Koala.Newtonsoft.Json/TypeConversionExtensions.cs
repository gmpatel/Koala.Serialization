using System;
using System.ComponentModel;

namespace Newtonsoft.Json
{
    public static class TypeConversionExtensions
    {
        public static T ChangeType<T>(this object value)
        {
            return (T)ChangeType(typeof(T), value);
        }

        public static object ChangeType(this Type t, object value)
        {
            var typeConverter = TypeDescriptor.GetConverter(t);
            return typeConverter.ConvertFrom(value);
        }

        public static void RegisterTypeConverter<T, TC>() where TC : TypeConverter
        {
            TypeDescriptor.AddAttributes(typeof(T), new TypeConverterAttribute(typeof(TC)));
        }
    }
}