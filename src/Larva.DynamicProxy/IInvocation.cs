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
        private object _val;

        public WrapperObject() { }

        public object Value
        {
            get { return _val; }
            set
            {
                _val = value;
                HasValue = true;
            }
        }

        public bool HasValue { get; private set; }
    }

    public enum MemberOperateTypes
    {
        Method = 0,

        PropertyGet = 1,

        PropertySet = 2
    }
}
