using System;

namespace App.Common.Scripts
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StringAttribute : Attribute, IValue<string>
    {
        public StringAttribute(string value)
        {
            Value = value;
        }
        
        public string Value
        {
            get;
        }
    }
}
