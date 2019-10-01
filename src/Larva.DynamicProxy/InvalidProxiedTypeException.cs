using System;

namespace Larva.DynamicProxy
{
    public class InvalidProxiedTypeException : Exception
    {
        public InvalidProxiedTypeException() { }

        public InvalidProxiedTypeException(string message) : base(message) { }

        public InvalidProxiedTypeException(string message, Exception innerException):base(message, innerException) { }
    }
}
