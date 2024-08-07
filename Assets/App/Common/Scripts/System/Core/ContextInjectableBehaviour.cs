using ContextSystem;
using UnityEngine;

namespace ContextSystem
{
    /// <summary>
    /// コンテキストを注入可能なMonoBehaviour
    /// </summary>
    public abstract class ContextInjectableBehaviour : MonoBehaviour, IContextInjectable
    {
        /// <summary>
        /// コンテキストの注入
        /// </summary>
        /// <param name="context">注入されるコンテキスト</param>
        /// <typeparam name="T">コンテキストの型</typeparam>
        public abstract void InjectContext<T>(T context) where T : IContext;
    }
}
