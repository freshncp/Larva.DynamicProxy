namespace Larva.DynamicProxy.PerfTests.Interceptors
{
    public class ValidateInterceptor : Larva.DynamicProxy.Interception.IInterceptor, Castle.DynamicProxy.IInterceptor
    {
        public void Intercept(Castle.DynamicProxy.IInvocation invocation)
        {
            if (invocation.Arguments[0].GetType() == typeof(int))
            {
                invocation.Arguments[0] = (int)invocation.Arguments[0] + 1;
            }
            invocation.Proceed();
        }

        public void Intercept(Larva.DynamicProxy.Interception.IInvocation invocation)
        {
            if (invocation.Arguments[0].GetType() == typeof(int))
            {
                invocation.Arguments[0] = (int)invocation.Arguments[0] + 1;
            }
            invocation.Proceed();
        }
    }
}
