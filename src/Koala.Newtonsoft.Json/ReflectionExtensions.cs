using System;
using System.Reflection;

namespace Newtonsoft.Json
{
    public static class ReflectionExtensions
    {
        public static T ObjectClone<T>(this T objSource)
        {
            var typeSource = objSource.GetType();
            var objTarget = Activator.CreateInstance(typeSource);

            var propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (PropertyInfo property in propertyInfo)
            {
                if (property.CanWrite)
                {
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    {
                        property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    }

                    else
                    {
                        var objPropertyValue = property.GetValue(objSource, null);

                        if (objPropertyValue == null)
                        {
                            property.SetValue(objTarget, null, null);
                        }
                        else
                        {
                            property.SetValue(objTarget, objPropertyValue.ObjectClone(), null);
                        }
                    }
                }
            }
            return (T)Convert.ChangeType(objTarget, typeof(T));
        }
    }
}