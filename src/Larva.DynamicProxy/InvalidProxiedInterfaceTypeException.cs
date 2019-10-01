using System;

namespace Larva.DynamicProxy
{
    public class InvalidProxiedInterfaceTypeException : Exception
    {
        public InvalidProxiedInterfaceTypeException() { }

        public InvalidProxiedInterfaceTypeException(string message) : base(message) { }

        public InvalidProxiedInterfaceTypeException(string message, Exception innerException):base(message, innerException) { }
    }
}
