using System;

namespace App.Common.Scripts
{
    [AttributeUsage(AttributeTargets.Field)]
    public class IntAttribute : Attribute, IValue<int>
    {
        public IntAttribute(int value)
        {
            Value = value;
        }
        
        public int Value
        {
            get;
        }
    }
}
