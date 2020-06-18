namespace Larva.DynamicProxy.PerfTests.Interceptors
{
    public class ValidateInterceptor : Larva.DynamicProxy.Interceptions.IInterceptor, Castle.DynamicProxy.IInterceptor
    {
        public void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            invocation.Proceed();
        }

        public void Intercept(Larva.DynamicProxy.Interceptions.IInvocation invocation)
        {
            invocation.Proceed();
        }
    }
}
