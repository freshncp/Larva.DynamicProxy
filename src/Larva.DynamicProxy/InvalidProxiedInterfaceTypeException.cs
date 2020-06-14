using System;

namespace Larva.DynamicProxy
{
    /// <summary>
    /// 非法代理接口类型异常
    /// </summary>
    public class InvalidProxiedInterfaceTypeException : Exception
    {
        /// <summary>
        /// 非法代理接口类型异常
        /// </summary>
        public InvalidProxiedInterfaceTypeException() { }

        /// <summary>
        /// 非法代理接口类型异常
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public InvalidProxiedInterfaceTypeException(string message) : base(message) { }

        /// <summary>
        /// 非法代理接口类型异常
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <returns></returns>
        public InvalidProxiedInterfaceTypeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
