using System;
using System.Collections.Generic;
using System.Reflection;

namespace Larva.DynamicProxy
{
    /// <summary>
    /// 属性的Set方法调用
    /// </summary>
    public sealed class PropertySetInvocation : InvocationBase
    {
        private Queue<IInterceptor> _interceptors;

        /// <summary>
        /// 属性的Set方法调用
        /// </summary>
        /// <param name="interceptors">拦截器</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="invocationTarget">调用目标</param>
        /// <param name="methodInvocationTargetFunc">调用目标方法回调</param>
        /// <param name="proxy">代理对象</param>
        /// <param name="propertyValue">属性值</param>
        public PropertySetInvocation(IInterceptor[] interceptors, string propertyName, Type propertyType, object invocationTarget, Action<object> methodInvocationTargetFunc, object proxy, object propertyValue)
            : base(interceptors, MemberTypes.Property, propertyName, MemberOperateTypes.PropertySet, new Type[] { propertyType }, typeof(void), invocationTarget, proxy, new object[] { propertyValue })
        {
            if (interceptors != null && interceptors.Length > 0)
            {
                _interceptors = new Queue<IInterceptor>(interceptors);
            }
            MethodInvocationTargetFunc = methodInvocationTargetFunc;
        }

        /// <summary>
        /// 目标方法回调
        /// </summary>
        public Action<object> MethodInvocationTargetFunc { get; private set; }

        /// <summary>
        /// 调用目标对象
        /// </summary>
        /// <returns></returns>
        protected override object InvokeInvocationTarget()
        {
            MethodInvocationTargetFunc.Invoke(Arguments[0]);
            return null;
        }
    }
}
