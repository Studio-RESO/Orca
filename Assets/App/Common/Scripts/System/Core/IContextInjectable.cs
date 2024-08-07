namespace ContextSystem
{
    /// <summary>
    /// コンテキストを注入可能なインターフェース
    /// </summary>
    public interface IContextInjectable
    {
        /// <summary>
        /// コンテキストの注入
        /// </summary>
        /// <param name="context">注入されるコンテキスト</param>
        /// <typeparam name="T">コンテキストの型</typeparam>
        void InjectContext<T>(T context) where T : IContext;
    }
}
