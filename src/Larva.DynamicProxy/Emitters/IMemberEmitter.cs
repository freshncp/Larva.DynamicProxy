using System.Reflection;

namespace Larva.DynamicProxy.Emitters
{
    public interface IMemberEmitter
    {
        void Emit(MemberInfo memberInfo);
    }
}
