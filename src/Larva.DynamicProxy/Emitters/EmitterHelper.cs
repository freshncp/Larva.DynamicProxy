using System;
using System.Linq;

namespace Larva.DynamicProxy.Emitters
{
    public static class EmitterHelper
    {
        public static IInterceptor[] CreateInterceptors(Type[] interceptorTypes)
        {
            return interceptorTypes == null || interceptorTypes.Length == 0 ? null : interceptorTypes.Select(m => (IInterceptor)Activator.CreateInstance(m)).ToArray();
        }
    }
}