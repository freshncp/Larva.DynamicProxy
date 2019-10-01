using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Larva.DynamicProxy.Emitters
{
    public sealed class ProxyPropertyEmitter : IMemberEmitter
    {
        private IProxyTypeGeneratorInfo _typeGeneratorInfo;

        public ProxyPropertyEmitter(IProxyTypeGeneratorInfo typeGeneratorInfo)
        {
            _typeGeneratorInfo = typeGeneratorInfo;
        }

        public void Emit(MemberInfo memberInfo)
        {
            var proxiedTypePropertyInfo = memberInfo as PropertyInfo;
            var property = _typeGeneratorInfo.Builder.DefineProperty(proxiedTypePropertyInfo.Name, PropertyAttributes.HasDefault, proxiedTypePropertyInfo.PropertyType, Type.EmptyTypes);
            var getSetAttr = MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig |
                MethodAttributes.Virtual |
                MethodAttributes.NewSlot |
                MethodAttributes.Final;

            var interceptorTypesField = _typeGeneratorInfo.InterceptorTypesField;
            var proxiedObjField = _typeGeneratorInfo.ProxiedObjField;

            if (proxiedTypePropertyInfo.CanRead)
            {
                var getMethod = _typeGeneratorInfo.Builder.DefineMethod("get_" + proxiedTypePropertyInfo.Name, getSetAttr, proxiedTypePropertyInfo.PropertyType, Type.EmptyTypes);
                var generator = getMethod.GetILGenerator();
                var interceptorsVar = generator.DeclareLocal(typeof(IInterceptor[]));
                var invocationTargetVar = generator.DeclareLocal(typeof(object));
                var methodInvocationTargetVar = generator.DeclareLocal(typeof(MethodInfo));
                var methodVar = generator.DeclareLocal(typeof(MethodInfo));
                var invocationVar = generator.DeclareLocal(typeof(IInvocation));

                // Create interceptors
                generator.Emit(OpCodes.Ldsfld, interceptorTypesField);
                generator.Emit(OpCodes.Call, typeof(EmitterHelper).GetMethod(nameof(EmitterHelper.CreateInterceptors), new Type[] { typeof(Type[]) }));
                generator.Emit(OpCodes.Stloc, interceptorsVar);

                // Get proxiedObj
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, proxiedObjField);
                generator.Emit(OpCodes.Stloc, invocationTargetVar);

                // Get proxiedObj's method
                generator.Emit(OpCodes.Ldtoken, proxiedTypePropertyInfo.GetMethod);
                generator.Emit(OpCodes.Ldtoken, proxiedTypePropertyInfo.GetMethod.DeclaringType);
                generator.Emit(OpCodes.Call, typeof(MethodBase).GetMethod(nameof(MethodBase.GetMethodFromHandle), new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }));
                generator.Emit(OpCodes.Stloc, methodInvocationTargetVar);

                // Get proxy's method
                generator.Emit(OpCodes.Ldtoken, getMethod);
                generator.Emit(OpCodes.Ldtoken, getMethod.DeclaringType);
                generator.Emit(OpCodes.Call, typeof(MethodBase).GetMethod(nameof(MethodBase.GetMethodFromHandle), new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }));
                generator.Emit(OpCodes.Stloc, methodVar);

                // Newobj DefaultInvocation
                generator.Emit(OpCodes.Ldloc, interceptorsVar);
                generator.Emit(OpCodes.Ldc_I4, (int)MemberTypes.Property);
                generator.Emit(OpCodes.Ldstr, proxiedTypePropertyInfo.Name);
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Ldnull);
                generator.Emit(OpCodes.Ldloc, invocationTargetVar);
                generator.Emit(OpCodes.Ldloc, methodInvocationTargetVar);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldloc, methodVar);
                generator.Emit(OpCodes.Newobj, typeof(DefaultInvocation).GetConstructor(new Type[] { typeof(IInterceptor[]), typeof(MemberTypes), typeof(string), typeof(MemberOperateTypes), typeof(object[]), typeof(object), typeof(MethodInfo), typeof(object), typeof(MethodInfo) }));
                generator.Emit(OpCodes.Stloc, invocationVar);

                // invocation.Proceed
                generator.Emit(OpCodes.Ldloc, invocationVar);
                generator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetMethod(nameof(IInvocation.Proceed)));

                var returnObj = generator.DeclareLocal(proxiedTypePropertyInfo.PropertyType);
                generator.Emit(OpCodes.Ldloc, invocationVar);
                generator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetProperty(nameof(IInvocation.ReturnValue)).GetMethod);
                generator.Emit(OpCodes.Callvirt, typeof(WrapperObject).GetProperty(nameof(WrapperObject.Value)).GetMethod);
                if (proxiedTypePropertyInfo.PropertyType.GetTypeInfo().IsValueType)
                {
                    generator.Emit(OpCodes.Unbox_Any, proxiedTypePropertyInfo.PropertyType);
                }
                else
                {
                    generator.Emit(OpCodes.Castclass, proxiedTypePropertyInfo.PropertyType);
                }
                generator.Emit(OpCodes.Ret);
                property.SetGetMethod(getMethod);
            }
            if (proxiedTypePropertyInfo.CanWrite)
            {
                var setMethod = _typeGeneratorInfo.Builder.DefineMethod("set_" + proxiedTypePropertyInfo.Name, getSetAttr, null, new Type[] { proxiedTypePropertyInfo.PropertyType });
                var generator = setMethod.GetILGenerator();
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
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Newarr, typeof(object));
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Ldarg_1);
                if (proxiedTypePropertyInfo.PropertyType.GetTypeInfo().IsValueType)
                {
                    generator.Emit(OpCodes.Box, proxiedTypePropertyInfo.PropertyType);
                }
                generator.Emit(OpCodes.Stelem_Ref);
                generator.Emit(OpCodes.Stloc, argObjArrayVar);

                // Get proxiedObj
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, proxiedObjField);
                generator.Emit(OpCodes.Stloc, invocationTargetVar);

                // Get proxiedObj's method
                generator.Emit(OpCodes.Ldtoken, proxiedTypePropertyInfo.SetMethod);
                generator.Emit(OpCodes.Ldtoken, proxiedTypePropertyInfo.SetMethod.DeclaringType);
                generator.Emit(OpCodes.Call, typeof(MethodBase).GetMethod(nameof(MethodBase.GetMethodFromHandle), new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }));
                generator.Emit(OpCodes.Stloc, methodInvocationTargetVar);

                // Get proxy's method
                generator.Emit(OpCodes.Ldtoken, setMethod);
                generator.Emit(OpCodes.Ldtoken, setMethod.DeclaringType);
                generator.Emit(OpCodes.Call, typeof(MethodBase).GetMethod(nameof(MethodBase.GetMethodFromHandle), new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }));
                generator.Emit(OpCodes.Stloc, methodVar);

                // Newobj DefaultInvocation
                generator.Emit(OpCodes.Ldloc, interceptorsVar);
                generator.Emit(OpCodes.Ldc_I4, (int)MemberTypes.Property);
                generator.Emit(OpCodes.Ldstr, proxiedTypePropertyInfo.Name);
                generator.Emit(OpCodes.Ldc_I4_2);
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

                generator.Emit(OpCodes.Ret);
                property.SetSetMethod(setMethod);
            }
        }
    }
}
