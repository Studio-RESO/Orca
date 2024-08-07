using UnityEngine;

namespace ContextSystem
{
    public static class ContextInjectableBehaviourFactory
    {
        public static T Create<T>(T prefab, Transform parent, IContext[] contexts, string name = "") where T : ContextInjectableBehaviour, new()
        {
            T component;

            if (prefab == null)
            {
                var go = new GameObject();
                go.transform.SetParent(parent);
                component = go.AddComponent<T>();
            }
            else
            {
                component = GameObject.Instantiate(prefab, parent);
            }

            if (name.Length > 0) component.name = name;
            
            foreach (var context in contexts)
            {
                component.InjectContext(context);
            }

            return component;
        }
    }
}
