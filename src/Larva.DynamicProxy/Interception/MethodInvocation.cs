using System;
using System.Reflection;

namespace Larva.DynamicProxy.Interception
{
    /// <summary>
    /// 方法调用
    /// </summary>
    public sealed class MethodInvocation : InvocationBase
    {
        private Func<object[], object> _methodInvocationTargetFunc;

        /// <summary>
        /// 方法调用
        /// </summary>
        /// <param name="interceptors">拦截器</param>
        /// <param name="methodName">方法名</param>
        /// <param name="argumentTypes">参数类型</param>
        /// <param name="genericArgumentTypes">泛型参数类型</param>
        /// <param name="returnValueType">返回值类型</param>
        /// <param name="invocationTarget">调用目标对象</param>
        /// <param name="methodInvocationTargetFunc">调用目标方法回调</param>
        /// <param name="proxy">代理对象</param>
        /// <param name="arguments">参数</param>
        public MethodInvocation(IInterceptor[] interceptors, string methodName, Type[] argumentTypes, Type[] genericArgumentTypes, Type returnValueType, object invocationTarget, Func<object[], object> methodInvocationTargetFunc, object proxy, object[] arguments)
            : base(interceptors, MemberTypes.Method, methodName, MemberOperateTypes.None, argumentTypes, genericArgumentTypes, returnValueType, invocationTarget, proxy, arguments)
        {
            _methodInvocationTargetFunc = methodInvocationTargetFunc;
        }

        /// <summary>
        /// 调用目标对象
        /// </summary>
        /// <returns></returns>
        protected override object InvokeInvocationTarget()
        {
            return _methodInvocationTargetFunc.Invoke(Arguments);
        }
    }
}
