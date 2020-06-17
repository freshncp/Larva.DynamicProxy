using System;
using System.Collections.Generic;
using System.Reflection;

namespace Larva.DynamicProxy.Interceptions
{
    /// <summary>
    /// 方法调用
    /// </summary>
    public sealed class MethodInvocation : InvocationBase
    {
        private Queue<IInterceptor> _interceptors;

        /// <summary>
        /// 方法调用
        /// </summary>
        /// <param name="interceptors">拦截器</param>
        /// <param name="methodName">方法名</param>
        /// <param name="argumentTypes">参数类型</param>
        /// <param name="returnValueType">返回值类型</param>
        /// <param name="invocationTarget">调用目标对象</param>
        /// <param name="methodInvocationTargetFunc">调用目标方法回调</param>
        /// <param name="proxy">代理对象</param>
        /// <param name="arguments">参数</param>
        public MethodInvocation(IInterceptor[] interceptors, string methodName, Type[] argumentTypes, Type returnValueType, object invocationTarget, Func<object[], object> methodInvocationTargetFunc, object proxy, object[] arguments)
            : base(interceptors, MemberTypes.Method, methodName, MemberOperateTypes.None, argumentTypes, returnValueType, invocationTarget, proxy, arguments)
        {
            if (interceptors != null && interceptors.Length > 0)
            {
                _interceptors = new Queue<IInterceptor>(interceptors);
            }
            MethodInvocationTargetFunc = methodInvocationTargetFunc;
        }

        /// <summary>
        /// 调用目标方法回调
        /// </summary>
        public Func<object[], object> MethodInvocationTargetFunc { get; private set; }

        /// <summary>
        /// 调用目标对象
        /// </summary>
        /// <returns></returns>
        protected override object InvokeInvocationTarget()
        {
            return MethodInvocationTargetFunc.Invoke(Arguments);
        }
    }
}
