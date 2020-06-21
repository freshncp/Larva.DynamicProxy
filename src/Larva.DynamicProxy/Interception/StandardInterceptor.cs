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
                catch (Exception ex)
                {
                    EatException(() => ExceptionThrown(invocation, ex));
                    if (ex is AggregateException)
                    {
                        throw ex;
                    }
                    else
                    {
                        throw new AggregateException(ex);
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
                            else
                            {
                                EatException(() => ExceptionThrown(((InvocationAndEventWaitHandle)state).Invocation, lastTask.Exception.InnerExceptions[0]));
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
                catch (Exception ex)
                {
                    EatException(() => ExceptionThrown(invocation, ex));
                    if (ex is AggregateException)
                    {
                        throw ex;
                    }
                    else
                    {
                        throw new AggregateException(ex);
                    }
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
        /// 调用时抛异常
        /// </summary>
        /// <param name="invocation">调用</param>
        /// <param name="exception">异常</param>
        protected virtual void ExceptionThrown(IInvocation invocation, Exception exception)
        {

        }

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