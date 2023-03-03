using Newtonsoft.Json.Extensions;
using Ardalis.SmartEnum;

namespace Newtonsoft.Json.SmartEnums
{
    public abstract class AbstractSmartEnum : SmartEnum<AbstractSmartEnum, int>
    {
        public static implicit operator string(AbstractSmartEnum abstractSmartEnum) => abstractSmartEnum.Key;

        public string Key => this.ToString();

        protected AbstractSmartEnum(string name, int value) : base(name, value)
        {
        }

        public override string ToString()
        {
            return $"{base.ToString()} [{nameof(AbstractSmartEnum)}, {this.GetType().FullName}]";
        }

        public static T FromString<T>(string value, T defaultValue = default) where T : AbstractSmartEnum
        {
            return value.ToSmartEnum<T>(defaultValue);
        }
    }
}