using System;
using System.Reflection;

namespace Larva.DynamicProxy.Interception
{
    /// <summary>
    /// 属性的Get方法调用
    /// </summary>
    public sealed class PropertyGetInvocation : InvocationBase
    {
        private Func<object> _getMethodInvocationTargetFunc;

        /// <summary>
        /// 属性的Get方法调用
        /// </summary>
        /// <param name="interceptors">拦截器</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="invocationTarget">调用目标对象</param>
        /// <param name="methodInvocationTargetFunc">调用目标方法回调</param>
        /// <param name="proxy">代理对象</param>
        public PropertyGetInvocation(IInterceptor[] interceptors, string propertyName, Type propertyType, object invocationTarget, Func<object> methodInvocationTargetFunc, object proxy)
            : base(interceptors, MemberTypes.Property, propertyName, MemberOperateTypes.PropertyGet, Type.EmptyTypes, Type.EmptyTypes, propertyType, invocationTarget, proxy, null)
        {
            _getMethodInvocationTargetFunc = methodInvocationTargetFunc;
        }

        /// <summary>
        /// 调用目标对象
        /// </summary>
        /// <returns></returns>
        protected override object InvokeInvocationTarget()
        {
            return _getMethodInvocationTargetFunc.Invoke();
        }
    }
}
