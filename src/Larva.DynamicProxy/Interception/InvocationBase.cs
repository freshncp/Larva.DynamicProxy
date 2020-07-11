using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Larva.DynamicProxy.Interception
{
    /// <summary>
    /// 调用 抽象类
    /// </summary>
    public abstract class InvocationBase : IInvocation
    {
        private Queue<IInterceptor> _interceptors;

        /// <summary>
        /// 调用 抽象类
        /// </summary>
        /// <param name="interceptors">拦截器</param>
        /// <param name="memberType">成员类型</param>
        /// <param name="memberName">成员名</param>
        /// <param name="memberOperateType">成员操作类型</param>
        /// <param name="argumentTypes">参数类型</param>
        /// <param name="genericArgumentTypes">泛型参数类型</param>
        /// <param name="returnValueType">返回类型</param>
        /// <param name="invocationTarget">调用目标对象</param>
        /// <param name="proxy">代理对象</param>
        /// <param name="arguments">参数</param>
        protected InvocationBase(IInterceptor[] interceptors, MemberTypes memberType, string memberName, MemberOperateTypes memberOperateType, Type[] argumentTypes, Type[] genericArgumentTypes, Type returnValueType, object invocationTarget, object proxy, object[] arguments)
        {
            if (interceptors != null && interceptors.Length > 0)
            {
                _interceptors = new Queue<IInterceptor>(interceptors.Where(w => w != null));
            }
            MemberType = memberType;
            MemberName = memberName;
            MemberOperateType = memberOperateType;
            ArgumentTypes = argumentTypes == null ? Type.EmptyTypes : argumentTypes;
            GenericArgumentTypes = genericArgumentTypes == null ? Type.EmptyTypes : genericArgumentTypes;
            ReturnValueType = returnValueType;
            InvocationTarget = invocationTarget;
            Proxy = proxy;
            Arguments = arguments;
        }

        /// <summary>
        /// 成员类型
        /// </summary>
        public MemberTypes MemberType { get; private set; }

        /// <summary>
        /// 成员名
        /// </summary>
        public string MemberName { get; private set; }

        /// <summary>
        /// 成员操作类型
        /// </summary>
        public MemberOperateTypes MemberOperateType { get; private set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public Type[] ArgumentTypes { get; private set; }

        /// <summary>
        /// 泛型参数类型
        /// </summary>
        public Type[] GenericArgumentTypes { get; private set; }

        /// <summary>
        /// 返回值类型
        /// </summary>
        public Type ReturnValueType { get; private set; }

        /// <summary>
        /// 调用目标对象
        /// </summary>
        public object InvocationTarget { get; private set; }

        /// <summary>
        /// 代理对象
        /// </summary>
        public object Proxy { get; private set; }

        /// <summary>
        /// 参数
        /// </summary>
        public object[] Arguments { get; private set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public object ReturnValue { get; private set; }

        /// <summary>
        /// 是否目标对象已调用
        /// </summary>
        public bool IsInvocationTargetInvocated { get; private set; }

        /// <summary>
        /// 上个已处理的拦截器类型（如果没有，即为Null）
        /// </summary>
        public Type LastProceedInterceptorType { get; private set; }

        /// <summary>
        /// 处理
        /// </summary>
        public void Proceed()
        {
            if (_interceptors != null && _interceptors.Count > 0)
            {
                var interceptor = _interceptors.Dequeue();
                interceptor.Intercept(this);
                LastProceedInterceptorType = interceptor.GetType();
            }
            else
            {
                ReturnValue = InvokeInvocationTarget();
                IsInvocationTargetInvocated = true;
            }
        }

        /// <summary>
        /// 调用目标对象
        /// </summary>
        /// <returns></returns>
        protected abstract object InvokeInvocationTarget();
    }
}