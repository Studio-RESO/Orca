using UnityEngine;

namespace Asteria
{
    internal static class InternalContext
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Setup()
        {
            Context = null;
        }
#endif
        public static IContext Context
        {
            get;
            set;
        }
    }
}
