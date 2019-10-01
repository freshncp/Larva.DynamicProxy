using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Larva.DynamicProxy.Emitters
{
    public sealed class ProxyMethodEmitter : IMemberEmitter
    {
        private IProxyTypeGeneratorInfo _typeGeneratorInfo;

        public ProxyMethodEmitter(IProxyTypeGeneratorInfo typeGeneratorInfo)
        {
            _typeGeneratorInfo = typeGeneratorInfo;
        }

        public void Emit(MemberInfo memberInfo)
        {
            var proxiedTypeMethodInfo = memberInfo as MethodInfo;
            Type[] paramTypes = proxiedTypeMethodInfo.GetParameters().Select(m => m.ParameterType).ToArray();
            var method = _typeGeneratorInfo.Builder.DefineMethod(proxiedTypeMethodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, proxiedTypeMethodInfo.ReturnType, paramTypes.ToArray());
            if (proxiedTypeMethodInfo.IsGenericMethod)
            {
                var genericArgs = proxiedTypeMethodInfo.GetGenericArguments();
                List<string> genericParameterNameList = new List<string>();
                for (var i = 0; i < genericArgs.Length; i++)
                {
                    genericParameterNameList.Add(string.Format("T{0}", i));
                }
                var genericParameters = method.DefineGenericParameters(genericParameterNameList.ToArray());
                for (var i = 0; i < genericArgs.Length; i++)
                {
                    var attrs = genericArgs[i].GetTypeInfo().GenericParameterAttributes;
                    genericParameters[i].SetGenericParameterAttributes(attrs);
                }
            }
            var interceptorTypesField = _typeGeneratorInfo.InterceptorTypesField;
            var proxiedObjField = _typeGeneratorInfo.ProxiedObjField;
            var generator = method.GetILGenerator();
            var interceptorsVar = generator.DeclareLocal(typeof(IInterceptor[]));
            var argObjArrayVar = generator.DeclareLocal(typeof(object[]));
            var invocationTargetVar = generator.DeclareLocal(typeof(object));
            var methodInvocationTargetVar = generator.DeclareLocal(typeof(MethodInfo));
            var methodVar = generator.DeclareLocal(typeof(MethodInfo));
            var invocationVar = generator.DeclareLocal(typeof(IInvocation));

            // Create interceptors
            generator.Emit(OpCodes.Ldsfld, interceptorTypesField);
            generator.Emit(OpCodes.Call, typeof(EmitterHelper).GetMethod(nameof(EmitterHelper.CreateInterceptors), new Type[] { typeof(Type[]) }));
            generator.Emit(OpCodes.Stloc, interceptorsVar);

            // Newarr argument.
            generator.Emit(OpCodes.Ldc_I4, paramTypes.Length);
            generator.Emit(OpCodes.Newarr, typeof(object));
            for (var i = 0; i < paramTypes.Length; i++)
            {
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Ldarg, i + 1);
                if (paramTypes[i].GetTypeInfo().IsValueType)
                {
                    generator.Emit(OpCodes.Box, paramTypes[i]);
                }
                generator.Emit(OpCodes.Stelem_Ref);
            }
            generator.Emit(OpCodes.Stloc, argObjArrayVar);

            // Get proxiedObj
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, proxiedObjField);
            generator.Emit(OpCodes.Stloc, invocationTargetVar);

            // Get proxiedObj's method
            generator.Emit(OpCodes.Ldtoken, proxiedTypeMethodInfo);
            generator.Emit(OpCodes.Ldtoken, proxiedTypeMethodInfo.DeclaringType);
            generator.Emit(OpCodes.Call, typeof(MethodBase).GetMethod(nameof(MethodBase.GetMethodFromHandle), new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }));
            generator.Emit(OpCodes.Stloc, methodInvocationTargetVar);

            // Get proxy's method
            generator.Emit(OpCodes.Ldtoken, method);
            generator.Emit(OpCodes.Ldtoken, method.DeclaringType);
            generator.Emit(OpCodes.Call, typeof(MethodBase).GetMethod(nameof(MethodBase.GetMethodFromHandle), new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }));
            generator.Emit(OpCodes.Stloc, methodVar);

            // Newobj DefaultInvocation
            generator.Emit(OpCodes.Ldloc, interceptorsVar);
            generator.Emit(OpCodes.Ldc_I4_8);
            generator.Emit(OpCodes.Ldstr, proxiedTypeMethodInfo.Name);
            generator.Emit(OpCodes.Ldc_I4_0);
            generator.Emit(OpCodes.Ldloc, argObjArrayVar);
            generator.Emit(OpCodes.Ldloc, invocationTargetVar);
            generator.Emit(OpCodes.Ldloc, methodInvocationTargetVar);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldloc, methodVar);
            generator.Emit(OpCodes.Newobj, typeof(DefaultInvocation).GetConstructor(new Type[] { typeof(IInterceptor[]), typeof(MemberTypes), typeof(string), typeof(MemberOperateTypes), typeof(object[]), typeof(object), typeof(MethodInfo), typeof(object), typeof(MethodInfo) }));
            generator.Emit(OpCodes.Stloc, invocationVar);

            // invocation.Proceed
            generator.Emit(OpCodes.Ldloc, invocationVar);
            generator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetMethod(nameof(IInvocation.Proceed)));

            if (proxiedTypeMethodInfo.ReturnType != typeof(void))
            {
                generator.Emit(OpCodes.Ldloc, invocationVar);
                generator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetProperty(nameof(IInvocation.ReturnValue)).GetMethod);
                generator.Emit(OpCodes.Callvirt, typeof(WrapperObject).GetProperty(nameof(WrapperObject.Value)).GetMethod);
                if (proxiedTypeMethodInfo.ReturnType.GetTypeInfo().IsValueType)
                {
                    generator.Emit(OpCodes.Unbox_Any, proxiedTypeMethodInfo.ReturnType);
                }
                else
                {
                    generator.Emit(OpCodes.Castclass, proxiedTypeMethodInfo.ReturnType);
                }
            }
            generator.Emit(OpCodes.Ret);
        }
    }
}
