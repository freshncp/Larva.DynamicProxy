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

    public class WrapperObject
    {
        public WrapperObject() { }

        public object Value { get; set; }
    }

    public enum MemberOperateTypes
    {
        Method = 0,

        PropertyGet = 1,

        PropertySet = 2
    }
}
