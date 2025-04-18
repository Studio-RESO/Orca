using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace Asteria
{
    public static class EnumExtension
    {
        public static IEnumerable<T> ToEnumerable<T>() where T : Enum
        {
            Assert.IsTrue(typeof(T).IsEnum, "not an enum");

            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static IEnumerable<T> ToEnumerable<T>(Func<T, bool> func) where T : Enum
        {
            Assert.IsTrue(typeof(T).IsEnum, "not an enum");

            return Enum.GetValues(typeof(T)).Cast<T>().Where(func);
        }
    }
}
