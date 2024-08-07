using UnityEngine;

namespace ContextSystem
{
    /// <summary>
    /// コンテキストを注入可能なMonoBehaviourを生成するファクトリクラス
    /// </summary>
    public static class ContextInjectableBehaviourFactory
    {
        /// <summary>
        /// コンテキストを注入可能なMonoBehaviourを生成する
        /// </summary>
        /// <param name="prefab">生成されるゲームオブジェクト</param>
        /// <param name="parent">生成先のトランスフォーム</param>
        /// <param name="contexts">注入するコンテキスト</param>
        /// <param name="name">生成されるゲームオブジェクトの名前</param>
        /// <typeparam name="T">生成されるゲームオブジェクトのコンポーネントの型</typeparam>
        /// <returns>生成されるゲームオブジェクトのコンポーネント</returns>
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
