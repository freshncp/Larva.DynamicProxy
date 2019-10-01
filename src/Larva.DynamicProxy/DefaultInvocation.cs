using System;
using System.Collections.Generic;
using System.Reflection;

namespace Larva.DynamicProxy
{
    public sealed class DefaultInvocation : IInvocation
    {
        private Queue<IInterceptor> _interceptors;

        public DefaultInvocation(IInterceptor[] interceptors, MemberTypes memberType, string memberName, MemberOperateTypes memberOperateType, object[] arguments, object invocationTarget, MethodInfo methodInvocationTarget, object proxy, MethodInfo method)
        {
            if (interceptors != null)
            {
                _interceptors = new Queue<IInterceptor>(interceptors);
            }
            MemberType = memberType;
            MemberName = memberName;
            MemberOperateType = memberOperateType;
            Arguments = arguments;
            InvocationTarget = invocationTarget;
            MethodInvocationTarget = methodInvocationTarget;
            Proxy = proxy;
            Method = method;
            ReturnValue = new WrapperObject();
        }

        private DefaultInvocation(IInterceptor[] interceptors, MemberTypes memberType, string memberName, MemberOperateTypes memberOperateType, object[] arguments, object invocationTarget, MethodInfo methodInvocationTarget, object proxy, MethodInfo method, WrapperObject returnValue)
        {
            if (interceptors != null)
            {
                _interceptors = new Queue<IInterceptor>(interceptors);
            }
            MemberType = memberType;
            MemberName = memberName;
            MemberOperateType = memberOperateType;
            Arguments = arguments;
            InvocationTarget = invocationTarget;
            MethodInvocationTarget = methodInvocationTarget;
            Proxy = proxy;
            Method = method;
            ReturnValue = returnValue;
        }

        public MemberTypes MemberType { get; private set; }

        public string MemberName { get; private set; }

        public MemberOperateTypes MemberOperateType { get; private set; }

        public object[] Arguments { get; private set; }

        public object InvocationTarget { get; private set; }

        public MethodInfo MethodInvocationTarget { get; private set; }

        public object Proxy { get; private set; }

        public MethodInfo Method { get; private set; }

        public WrapperObject ReturnValue { get; private set; }

        public void Proceed()
        {
            if (_interceptors != null && _interceptors.Count > 0)
            {
                var interceptor = _interceptors.Dequeue();
                interceptor.Intercept(new DefaultInvocation(_interceptors.ToArray(), MemberType, MemberName, MemberOperateType, Arguments, InvocationTarget, MethodInvocationTarget, Proxy, Method, ReturnValue));
            }
            else if (MethodInvocationTarget != null)
            {
                try
                {
                    ReturnValue.Value = MethodInvocationTarget.Invoke(InvocationTarget, Arguments);
                }
                catch (Exception ex)
                {
                    var ctor = ex.InnerException.GetType().GetConstructor(new Type[] { typeof(string), typeof(Exception) });
                    if (ctor != null)
                    {
                        throw (Exception)ctor.Invoke(new object[] { ex.InnerException.Message, ex.InnerException });
                    }
                    throw new TargetInvocationException(ex.InnerException.Message, ex.InnerException);
                }
            }
        }
    }
}
