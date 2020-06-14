using System.Reflection;

namespace Larva.DynamicProxy.Emitters
{
    /// <summary>
    /// 成员 IL生成器
    /// </summary>
    public interface IMemberEmitter
    {
        /// <summary>
        /// 生成
        /// </summary>
        /// <param name="memberInfo"></param>
        void Emit(MemberInfo memberInfo);
    }
}
