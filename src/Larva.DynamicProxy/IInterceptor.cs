namespace Larva.DynamicProxy
{
    public interface IInterceptor
    {
        void Intercept(IInvocation invocation);
    }
}
