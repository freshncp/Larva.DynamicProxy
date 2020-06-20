namespace Larva.DynamicProxy.PerfTests.Interceptors
{
    public class ValidateInterceptor : Larva.DynamicProxy.Interception.IInterceptor, Castle.DynamicProxy.IInterceptor
    {
        public void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            invocation.Proceed();
        }

        public void Intercept(Larva.DynamicProxy.Interception.IInvocation invocation)
        {
            invocation.Proceed();
        }
    }
}
