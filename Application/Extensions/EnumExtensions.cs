using System;
using System.Linq;
using System.Reflection;
using Application.Types;

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
    }
}
