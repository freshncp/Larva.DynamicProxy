using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Larva.DynamicProxy
{
    public abstract class StandardInterceptor : IInterceptor, IDisposable
    {
        public void Intercept(IInvocation invocation)
        {   
            if (typeof(Task).GetTypeInfo().IsAssignableFrom(invocation.MethodInvocationTarget.ReturnType))
            {
                var isFailBeforePostProceed = true;
                try
                {
                    PreProceed(invocation);
                    invocation.Proceed();
                    isFailBeforePostProceed = false;
                    if (!invocation.ReturnValue.HasValue)
                    {
                        PostProceed(invocation);
                    }
                }
                finally
                {
                    if (isFailBeforePostProceed
                        || !invocation.ReturnValue.HasValue)
                    {
                        Dispose();
                    }
                }
                if (!isFailBeforePostProceed
                    && invocation.ReturnValue.HasValue)
                {
                    ((Task)invocation.ReturnValue.Value).ContinueWith((lastTask, state) =>
                    {
                        if (lastTask.Exception == null)
                        {
                            PostProceed((IInvocation)state);
                        }
                    }, invocation).ContinueWith((lastTask) =>
                    {
                        Dispose();
                    });
                }
            }
            else
            {
                try
                {
                    PreProceed(invocation);
                    invocation.Proceed();
                    PostProceed(invocation);
                }
                finally
                {
                    Dispose();
                }
            }
        }

        protected abstract void PreProceed(IInvocation invocation);

        protected abstract void PostProceed(IInvocation invocation);

        public virtual void Dispose()
        {
            
        }
    }
}
