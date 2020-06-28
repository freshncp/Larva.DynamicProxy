using System;

namespace Larva.DynamicProxy
{
    /// <summary>
    /// 调用未被处理异常
    /// </summary>
    public class InvocationNotProceedException : Exception
    {
        /// <summary>
        /// 调用未被处理异常
        /// </summary>
        public InvocationNotProceedException() { }

        /// <summary>
        /// 调用未被处理异常
        /// </summary>
        /// <param name="lastProceedInterceptorType"></param>
        /// <returns></returns>
        public InvocationNotProceedException(Type lastProceedInterceptorType)
            : base($"Interceptor \"{lastProceedInterceptorType.FullName}\" not invoke method \"Proceed\" of IInvocation.")
        {
            LastProceedInterceptorType = lastProceedInterceptorType;
        }

        /// <summary>
        /// 调用未被处理异常
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public InvocationNotProceedException(string message) : base(message) { }

        /// <summary>
        /// 调用未被处理异常
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <returns></returns>
        public InvocationNotProceedException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// 上个已处理的拦截器类型
        /// </summary>
        public Type LastProceedInterceptorType { get; private set; }
    }
}
