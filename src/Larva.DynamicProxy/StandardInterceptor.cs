using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Larva.DynamicProxy
{
    public abstract class StandardInterceptor : IInterceptor, IDisposable
    {
        public void Intercept(IInvocation invocation)
        {
            PreProceed(invocation);
            try
            {
                invocation.Proceed();
                if (typeof(Task).GetTypeInfo().IsAssignableFrom(invocation.MethodInvocationTarget.ReturnType)
                    && invocation.ReturnValue.HasValue)
                {
                    ((Task)invocation.ReturnValue.Value).ContinueWith((lastTask, state) =>
                    {
                        PostProceed((IInvocation)state);
                    }, invocation);
                }
                else
                {
                    PostProceed(invocation);
                }
            }
            finally
            {
                Dispose();
            }
        }

        protected abstract void PreProceed(IInvocation invocation);

        protected abstract void PostProceed(IInvocation invocation);

        public virtual void Dispose()
        {
            
        }
    }
}