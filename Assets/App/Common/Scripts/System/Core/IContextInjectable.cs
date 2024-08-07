namespace ContextSystem
{
    public interface IContextInjectable
    {
        void InjectContext<T>(T context) where T : IContext;
    }
}
