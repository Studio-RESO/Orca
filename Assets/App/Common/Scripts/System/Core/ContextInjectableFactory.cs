using UnityEngine;

namespace ContextSystem
{
    public static class ContextInjectableFactory
    {
        public static T Create<T>(T prefab, IContext[] contexts, string name = "") where T : ContextInjectableBehaviour, new()
        {
            T component = prefab== null ? new GameObject().AddComponent<T>() : GameObject.Instantiate(prefab);

            if (name.Length > 0) component.name = name;
            
            foreach (var context in contexts)
            {
                component.InjectContext(context);
            }

            return component;
        }
    }
}
