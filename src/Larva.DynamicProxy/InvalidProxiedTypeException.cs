using System;

namespace Larva.DynamicProxy
{
    /// <summary>
    /// 非法代理类型异常
    /// </summary>
    public class InvalidProxiedTypeException : Exception
    {
        /// <summary>
        /// 非法代理类型异常
        /// </summary>
        public InvalidProxiedTypeException() { }

        /// <summary>
        /// 非法代理类型异常
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public InvalidProxiedTypeException(string message) : base(message) { }

        /// <summary>
        /// 非法代理类型异常
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <returns></returns>
        public InvalidProxiedTypeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
