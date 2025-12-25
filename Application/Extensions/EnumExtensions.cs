using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Application.Attributes.Attributes;

namespace Application.Extensions
{
    public static class EnumExtensions
    {
        public static string GetMeta(this Enum value, string key)
        {
            var field = value.GetType().GetField(value.ToString());

            if (field == null)
                return value.ToString();

            var attribute = field.GetCustomAttributes<EnumMetaAttribute>()
                                 .FirstOrDefault(a => a.Key == key);

            return attribute?.Value ?? "";
        }

        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();
            
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }
    }
}
