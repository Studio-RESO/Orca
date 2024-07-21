using System.Collections.Generic;
using System.Linq;

namespace App.Common.Scripts
{
    internal interface IValue<T>
    {
        T Value
        {
            get;
        }
    }
    
    public class AttributeCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> cache;
        
        public AttributeCache()
        {
            var type = typeof(TKey);
            
            cache = type.GetFields()
                .Where(fi => fi.FieldType == type)
                .SelectMany(fi => fi.GetCustomAttributes(false), (fi, Attribute) => new { Type = (TKey) fi.GetValue(null), Attribute })
                .ToDictionary(k => k.Type, v => ((IValue<TValue>) v.Attribute).Value);
        }
        
        public TValue this[TKey t] => cache[t];
        
        public bool TryGetKey(TValue valeu, out TKey key)
        {
            foreach (var c in cache)
            {
                if (c.Value.Equals(valeu))
                {
                    key = c.Key;
                    
                    return true;
                }
            }
            key = default;
            
            return false;
        }
    }
}
