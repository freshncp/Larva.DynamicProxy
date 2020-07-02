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
                        ((Task)invocation.ReturnValue).ContinueWith((lastTask, state) =>
                        {
                            var invocationState = (InvocationState)state;
                            if (lastTask.Exception == null)
                            {
                                EatException(() => PostProceed(invocationState.Invocation));
                            }
                            ExecutionContext.Run(invocationState.MainThreadExecutionContext, (state2) =>
                            {
                                EatException(() => CleanProceed());
                            }, invocationState);
                        }, new InvocationState(invocation, ExecutionContext.Capture())).ConfigureAwait(false);
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

        private class InvocationState
        {
            public InvocationState(IInvocation invocation, ExecutionContext mainThreadExecutionContext)
            {
                Invocation = invocation;
                MainThreadExecutionContext = mainThreadExecutionContext;
            }

            public IInvocation Invocation { get; private set; }

            public ExecutionContext MainThreadExecutionContext { get; private set; }
        }
    }
}
