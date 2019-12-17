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
            var parameters = proxiedTypeMethodInfo.GetParameters();
            var method = _typeGeneratorInfo.Builder.DefineMethod(proxiedTypeMethodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, proxiedTypeMethodInfo.ReturnType, parameters.Select(m => m.ParameterType).ToArray());
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
            for (var i = 0; i < parameters.Length; i++)
            {
                method.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);
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
            generator.Emit(OpCodes.Call, typeof(EmitterHelper).GetTypeInfo().GetMethod(nameof(EmitterHelper.CreateInterceptors), new Type[] { typeof(Type[]) }));
            generator.Emit(OpCodes.Stloc, interceptorsVar);

            // Newarr argument.
            generator.Emit(OpCodes.Ldc_I4, parameters.Length);
            generator.Emit(OpCodes.Newarr, typeof(object));
            for (var i = 0; i < parameters.Length; i++)
            {
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Ldarg, i + 1);
                var unwrappedParameterType = parameters[i].ParameterType;
                if (parameters[i].ParameterType.IsByRef)
                {
                    unwrappedParameterType = parameters[i].ParameterType.GetElementType();
                    generator.Emit(OpCodes.Ldind_Ref);
                }
                if (unwrappedParameterType.GetTypeInfo().IsValueType)
                {
                    generator.Emit(OpCodes.Box, unwrappedParameterType);
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
            generator.Emit(OpCodes.Call, typeof(MethodBase).GetTypeInfo().GetMethod(nameof(MethodBase.GetMethodFromHandle), new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }));
            generator.Emit(OpCodes.Stloc, methodInvocationTargetVar);

            // Get proxy's method
            generator.Emit(OpCodes.Ldtoken, method);
            generator.Emit(OpCodes.Ldtoken, method.DeclaringType);
            generator.Emit(OpCodes.Call, typeof(MethodBase).GetTypeInfo().GetMethod(nameof(MethodBase.GetMethodFromHandle), new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }));
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
            generator.Emit(OpCodes.Newobj, typeof(DefaultInvocation).GetTypeInfo().GetConstructor(new Type[] { typeof(IInterceptor[]), typeof(MemberTypes), typeof(string), typeof(MemberOperateTypes), typeof(object[]), typeof(object), typeof(MethodInfo), typeof(object), typeof(MethodInfo) }));
            generator.Emit(OpCodes.Stloc, invocationVar);

            // invocation.Proceed
            generator.Emit(OpCodes.Ldloc, invocationVar);
            generator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetTypeInfo().GetMethod(nameof(IInvocation.Proceed)));

            // assign ref/out parameter
            for (var i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].ParameterType.IsByRef)
                {
                    continue;
                }
                var unwrappedParameterType = parameters[i].ParameterType.GetElementType();
                generator.Emit(OpCodes.Ldarg, i + 1);
                generator.Emit(OpCodes.Ldloc, argObjArrayVar);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Ldelem_Ref);
                if (unwrappedParameterType.GetTypeInfo().IsValueType)
                {
                    generator.Emit(OpCodes.Unbox_Any, unwrappedParameterType);
                }
                generator.Emit(OpCodes.Stind_Ref);
            }

            if (proxiedTypeMethodInfo.ReturnType != typeof(void))
            {
                generator.Emit(OpCodes.Ldloc, invocationVar);
                generator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetTypeInfo().GetProperty(nameof(IInvocation.ReturnValue)).GetMethod);
                generator.Emit(OpCodes.Callvirt, typeof(WrapperObject).GetTypeInfo().GetProperty(nameof(WrapperObject.Value)).GetMethod);
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
