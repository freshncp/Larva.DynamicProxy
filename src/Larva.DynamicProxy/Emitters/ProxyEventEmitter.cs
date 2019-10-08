using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Larva.DynamicProxy.Emitters
{
    public sealed class ProxyEventEmitter : IMemberEmitter
    {
        private IProxyTypeGeneratorInfo _typeGeneratorInfo;

        public ProxyEventEmitter(IProxyTypeGeneratorInfo typeGeneratorInfo)
        {
            _typeGeneratorInfo = typeGeneratorInfo;
        }
        
        public void Emit(MemberInfo memberInfo)
        {
            var proxiedTypeEventInfo = memberInfo as EventInfo;
            var @event = _typeGeneratorInfo.Builder.DefineEvent(proxiedTypeEventInfo.Name, EventAttributes.None, proxiedTypeEventInfo.EventHandlerType);
            var addRemoveAttr = MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig |
                MethodAttributes.Virtual |
                MethodAttributes.NewSlot |
                MethodAttributes.Final;

            if (proxiedTypeEventInfo.AddMethod != null)
            {
                var addMethod = _typeGeneratorInfo.Builder.DefineMethod("add_" + proxiedTypeEventInfo.Name, addRemoveAttr, typeof(void), new Type[] { proxiedTypeEventInfo.EventHandlerType });
                var generator = addMethod.GetILGenerator();

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, _typeGeneratorInfo.ProxiedObjField);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Call, proxiedTypeEventInfo.AddMethod);

                generator.Emit(OpCodes.Ret);
                @event.SetAddOnMethod(addMethod);
            }

            if (proxiedTypeEventInfo.RemoveMethod != null)
            {
                var removeMethod = _typeGeneratorInfo.Builder.DefineMethod("remove_" + proxiedTypeEventInfo.Name, addRemoveAttr, typeof(void), new Type[] { proxiedTypeEventInfo.EventHandlerType });
                var generator = removeMethod.GetILGenerator();

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, _typeGeneratorInfo.ProxiedObjField);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Call, proxiedTypeEventInfo.RemoveMethod);

                generator.Emit(OpCodes.Ret);
                @event.SetRemoveOnMethod(removeMethod);
            }

            if (proxiedTypeEventInfo.RaiseMethod != null)
            {
                var raiseMethod = _typeGeneratorInfo.Builder.DefineMethod("raise_" + proxiedTypeEventInfo.Name, addRemoveAttr, typeof(void), new Type[] { proxiedTypeEventInfo.EventHandlerType });
                var generator = raiseMethod.GetILGenerator();

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, _typeGeneratorInfo.ProxiedObjField);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Call, proxiedTypeEventInfo.RaiseMethod);

                generator.Emit(OpCodes.Ret);
                @event.SetRaiseMethod(raiseMethod);
            }
        }
    }
}
