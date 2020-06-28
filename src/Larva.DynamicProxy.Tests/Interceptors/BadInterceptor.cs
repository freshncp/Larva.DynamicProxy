using Larva.DynamicProxy.Interception;

namespace Larva.DynamicProxy.Tests
{
    public class BadInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            // Miss invocation.Proceed();
        }
    }
}