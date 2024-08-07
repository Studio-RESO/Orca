using ContextSystem;
using UnityEngine;

namespace ContextSystem
{
    public abstract class ContextInjectableBehaviour : MonoBehaviour, IContextInjectable
    {
        public abstract void InjectContext<T>(T context) where T : IContext;
    }
}
