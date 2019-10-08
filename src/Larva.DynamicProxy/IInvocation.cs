using System.Reflection;

namespace Larva.DynamicProxy
{
    public interface IInvocation
    {
        MemberTypes MemberType { get; }

        string MemberName { get; }

        MemberOperateTypes MemberOperateType { get; }

        object InvocationTarget { get; }

        MethodInfo MethodInvocationTarget { get; }

        object Proxy { get; }

        MethodInfo Method { get; }

        object[] Arguments { get; }

        WrapperObject ReturnValue { get; }

        void Proceed();
    }
}
