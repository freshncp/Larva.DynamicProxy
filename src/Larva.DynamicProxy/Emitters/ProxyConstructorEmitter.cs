﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Larva.DynamicProxy.Emitters
{
    public sealed class ProxyConstructorEmitter : IMemberEmitter
    {
        private IProxyTypeGeneratorInfo _typeGeneratorInfo;

        public ProxyConstructorEmitter(IProxyTypeGeneratorInfo typeGeneratorInfo)
        {
            _typeGeneratorInfo = typeGeneratorInfo;
        }

        public void Emit(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                var cctor = _typeGeneratorInfo.Builder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new Type[] { _typeGeneratorInfo.ProxiedType });
                var proxiedObjField = _typeGeneratorInfo.ProxiedObjField;
                var generator = cctor.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Stfld, proxiedObjField);
                generator.Emit(OpCodes.Ret);
            }
            else
            {
                var proxiedTypeConstructorInfo = memberInfo as ConstructorInfo;
                Type[] paramTypes = proxiedTypeConstructorInfo.GetParameters().Select(m => m.ParameterType).ToArray();
                var cctor = _typeGeneratorInfo.Builder.DefineConstructor(proxiedTypeConstructorInfo.Attributes, proxiedTypeConstructorInfo.CallingConvention, paramTypes);
                var proxiedObjField = _typeGeneratorInfo.ProxiedObjField;
                var generator = cctor.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                for (var i = 0; i < paramTypes.Length; i++)
                {
                    generator.Emit(OpCodes.Ldarg, i + 1);
                }
                generator.Emit(OpCodes.Newobj, proxiedTypeConstructorInfo);
                generator.Emit(OpCodes.Stfld, proxiedObjField);
                generator.Emit(OpCodes.Ret);
            }
        }
    }
}
