using System;
using System.Threading;
using System.Threading.Tasks;

namespace Larva.DynamicProxy.Interception
{
    /// <summary>
    /// 标准拦截器
    /// </summary>
    public abstract class StandardInterceptor : IInterceptor
    {
        /// <summary>
        /// 拦截
        /// </summary>
        /// <param name="invocation">调用</param>
        public void Intercept(IInvocation invocation)
        {
            if (typeof(Task).IsAssignableFrom(invocation.ReturnValueType))
            {
                var isFailBeforePostProceed = true;
                try
                {
                    PreProceed(invocation);
                    invocation.Proceed();
                    isFailBeforePostProceed = false;
                    if (!invocation.IsInvocationTargetInvocated)
                    {
                        EatException(() => PostProceed(invocation));
                    }
                }
                finally
                {
                    if (isFailBeforePostProceed
                        || !invocation.IsInvocationTargetInvocated)
                    {
                        EatException(() => CleanProceed());
                    }
                }
                if (!isFailBeforePostProceed
                    && invocation.IsInvocationTargetInvocated)
                {
                    if (invocation.ReturnValue == null)
                    {
                        EatException(() => CleanProceed());
                    }
                    else
                    {
                        var waitPostProceedOrExceptionThrown = new ManualResetEvent(false);
                        ((Task)invocation.ReturnValue).ContinueWith((lastTask, state) =>
                        {
                            if (lastTask.Exception == null)
                            {
                                EatException(() => PostProceed(((InvocationAndEventWaitHandle)state).Invocation));
                            }
                            ((InvocationAndEventWaitHandle)state).WaitHandle.Set();
                        }, new InvocationAndEventWaitHandle(invocation, waitPostProceedOrExceptionThrown)).ConfigureAwait(false);
                        waitPostProceedOrExceptionThrown.WaitOne();
                        EatException(() => CleanProceed());
                    }
                }
            }
            else
            {
                try
                {
                    PreProceed(invocation);
                    invocation.Proceed();
                    EatException(() => PostProceed(invocation));
                }
                finally
                {
                    EatException(() => CleanProceed());
                }
            }
        }

        /// <summary>
        /// 调用前
        /// </summary>
        /// <param name="invocation">调用</param>
        protected abstract void PreProceed(IInvocation invocation);

        /// <summary>
        /// 调用后
        /// </summary>
        /// <param name="invocation">调用</param>
        protected abstract void PostProceed(IInvocation invocation);

        /// <summary>
        /// 调用结束的清理
        /// </summary>
        protected virtual void CleanProceed()
        {

        }

        private void EatException(Action action)
        {
            try
            {
                action();
            }
            catch { }
        }

        private class InvocationAndEventWaitHandle
        {
            public InvocationAndEventWaitHandle(IInvocation invocation, EventWaitHandle waitHandle)
            {
                Invocation = invocation;
                WaitHandle = waitHandle;
            }

            public IInvocation Invocation { get; private set; }

            public EventWaitHandle WaitHandle { get; private set; }
        }
    }
}
