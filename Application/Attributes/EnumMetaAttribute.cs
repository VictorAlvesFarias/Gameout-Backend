using System;

namespace Application.Types
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnumMetaAttribute : Attribute
    {
        public string Key { get; }
        public string Value { get; }

        public EnumMetaAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
