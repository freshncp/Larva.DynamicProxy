using System;
using System.Reflection;

namespace Larva.DynamicProxy.Interception
{
    /// <summary>
    /// 调用
    /// </summary>
    public interface IInvocation
    {
        /// <summary>
        /// 成员类型
        /// </summary>
        MemberTypes MemberType { get; }

        /// <summary>
        /// 成员名称
        /// </summary>
        string MemberName { get; }

        /// <summary>
        /// 成员操作类型
        /// </summary>
        MemberOperateTypes MemberOperateType { get; }

        /// <summary>
        /// 参数类型
        /// </summary>
        Type[] ArgumentTypes { get; }

        /// <summary>
        /// 泛型参数类型
        /// </summary>
        Type[] GenericArgumentTypes { get; }

        /// <summary>
        /// 返回值类型
        /// </summary>
        Type ReturnValueType { get; }

        /// <summary>
        /// 调用目标对象
        /// </summary>
        object InvocationTarget { get; }

        /// <summary>
        /// 代理对象
        /// </summary>
        object Proxy { get; }

        /// <summary>
        /// 参数
        /// </summary>
        object[] Arguments { get; }

        /// <summary>
        /// 返回值
        /// </summary>
        object ReturnValue { get; }

        /// <summary>
        /// 是否目标对象已调用
        /// </summary>
        bool IsInvocationTargetInvocated { get; }
        
        /// <summary>
        /// 上个已处理的拦截器类型（如果没有，即为Null）
        /// </summary>
        Type LastProceedInterceptorType { get; }

        /// <summary>
        /// 处理
        /// </summary>
        void Proceed();
    }
}
