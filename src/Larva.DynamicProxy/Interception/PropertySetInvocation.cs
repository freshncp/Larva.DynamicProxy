using System;
using System.Reflection;

namespace Larva.DynamicProxy.Interception
{
    /// <summary>
    /// 属性的Set方法调用
    /// </summary>
    public sealed class PropertySetInvocation : InvocationBase
    {
        private Action<object> _setMethodInvocationTargetFunc;

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
            : base(interceptors, MemberTypes.Property, propertyName, MemberOperateTypes.PropertySet, new Type[] { propertyType }, Type.EmptyTypes, typeof(void), invocationTarget, proxy, new object[] { propertyValue })
        {
            _setMethodInvocationTargetFunc = methodInvocationTargetFunc;
        }

        /// <summary>
        /// 调用目标对象
        /// </summary>
        /// <returns></returns>
        protected override object InvokeInvocationTarget()
        {
            _setMethodInvocationTargetFunc.Invoke(Arguments[0]);
            return null;
        }
    }
}
