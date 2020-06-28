using Larva.DynamicProxy.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Larva.DynamicProxy.Emitters
{
    /// <summary>
    /// 代理类方法 IL生成器
    /// </summary>
    public sealed class ProxyMethodEmitter : IMemberEmitter
    {
        private IProxyTypeGeneratorInfo _typeGeneratorInfo;

        /// <summary>
        /// 代理类方法 IL生成器
        /// </summary>
        /// <param name="typeGeneratorInfo"></param>
        public ProxyMethodEmitter(IProxyTypeGeneratorInfo typeGeneratorInfo)
        {
            _typeGeneratorInfo = typeGeneratorInfo;
        }

        /// <summary>
        /// 生成
        /// </summary>
        /// <param name="memberInfo"></param>
        public void Emit(MemberInfo memberInfo)
        {
            var proxiedTypeMethodInfo = memberInfo as MethodInfo;
            var parameters = proxiedTypeMethodInfo.GetParameters();
            var proxiedObjField = _typeGeneratorInfo.ProxiedObjField;
            var interceptorsField = _typeGeneratorInfo.InterceptorsField;

            #region 私有方法，用于生成目标方法回调

            var methodFunc = _typeGeneratorInfo.Builder.DefineMethod($"__{proxiedTypeMethodInfo.Name}__", MethodAttributes.Private, typeof(object), new Type[] { typeof(object[]) });
            GenericTypeParameterBuilder[] methodFuncGenericParameters = null;
            if (proxiedTypeMethodInfo.IsGenericMethod)
            {
                var genericParameterNameList = new List<string>();
                var genericArgs = proxiedTypeMethodInfo.GetGenericArguments();
                for (var i = 0; i < genericArgs.Length; i++)
                {
                    genericParameterNameList.Add(string.Format("T{0}", i));
                }
                methodFuncGenericParameters = methodFunc.DefineGenericParameters(genericParameterNameList.ToArray());
                for (var i = 0; i < genericArgs.Length; i++)
                {
                    methodFuncGenericParameters[i].SetGenericParameterAttributes(genericArgs[i].GenericParameterAttributes);
                    methodFuncGenericParameters[i].SetInterfaceConstraints(genericArgs[i].GetInterfaces());
                    methodFuncGenericParameters[i].SetBaseTypeConstraint(genericArgs[i].BaseType);
                }
            }
            var methodFuncGenerator = methodFunc.GetILGenerator();

            // 参数列表Copy到局部变量
            var methodFuncArgumentVarList = new LocalBuilder[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].ParameterType.IsByRef)
                {
                    continue;
                }
                var unwrappedParameterType = parameters[i].ParameterType.GetElementType();
                methodFuncGenerator.Ldarg(1);
                methodFuncGenerator.Ldc_I4(i);
                methodFuncGenerator.Emit(OpCodes.Ldelem_Ref);
                if (unwrappedParameterType.IsValueType)
                {
                    methodFuncGenerator.Emit(OpCodes.Unbox_Any, unwrappedParameterType);
                }
                var methodFuncArgumentVar = methodFuncGenerator.DeclareLocal(unwrappedParameterType);
                methodFuncGenerator.Emit(OpCodes.Stloc, methodFuncArgumentVar);
                methodFuncArgumentVarList[i] = methodFuncArgumentVar;
            }

            // 获取返回值，无返回类型的方法，返回值设为null
            methodFuncGenerator.Ldarg(0);
            methodFuncGenerator.Emit(OpCodes.Ldfld, proxiedObjField);
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.IsByRef)
                {
                    methodFuncGenerator.Emit(OpCodes.Ldloca_S, methodFuncArgumentVarList[i]);
                }
                else
                {
                    methodFuncGenerator.Ldarg(1);
                    methodFuncGenerator.Ldc_I4(i);
                    methodFuncGenerator.Emit(OpCodes.Ldelem_Ref);
                    if (parameters[i].ParameterType.IsValueType)
                    {
                        methodFuncGenerator.Emit(OpCodes.Unbox_Any, parameters[i].ParameterType);
                    }
                }
            }
            if (proxiedTypeMethodInfo.IsGenericMethod)
            {
                methodFuncGenerator.Emit(OpCodes.Callvirt, proxiedTypeMethodInfo.MakeGenericMethod(methodFuncGenericParameters));
            }
            else
            {
                methodFuncGenerator.Emit(OpCodes.Callvirt, proxiedTypeMethodInfo);
            }
            LocalBuilder methodFuncReturnValueVal = null;
            if (proxiedTypeMethodInfo.ReturnType != typeof(void))
            {
                methodFuncReturnValueVal = methodFuncGenerator.DeclareLocal(typeof(object));
                if (proxiedTypeMethodInfo.ReturnType.IsValueType)
                {
                    methodFuncGenerator.Emit(OpCodes.Box, proxiedTypeMethodInfo.ReturnType);
                }
                methodFuncGenerator.Emit(OpCodes.Stloc, methodFuncReturnValueVal);
            }

            // ref/out 参数赋值
            for (var i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].ParameterType.IsByRef)
                {
                    continue;
                }

                methodFuncGenerator.Ldarg(1);
                methodFuncGenerator.Ldc_I4(i);
                methodFuncGenerator.Emit(OpCodes.Ldloc, methodFuncArgumentVarList[i]);
                if (methodFuncArgumentVarList[i].LocalType.IsValueType)
                {
                    methodFuncGenerator.Emit(OpCodes.Box, methodFuncArgumentVarList[i].LocalType);
                }
                methodFuncGenerator.Emit(OpCodes.Stelem_Ref);
            }
            if (methodFuncReturnValueVal == null)
            {
                methodFuncGenerator.Emit(OpCodes.Ldnull);
            }
            else
            {
                methodFuncGenerator.Emit(OpCodes.Ldloc, methodFuncReturnValueVal);
            }
            methodFuncGenerator.Emit(OpCodes.Ret);

            #endregion

            #region 代理方法

            var method = _typeGeneratorInfo.Builder.DefineMethod(proxiedTypeMethodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, proxiedTypeMethodInfo.ReturnType, parameters.Select(m => m.ParameterType).ToArray());
            GenericTypeParameterBuilder[] methodGenericParameters = null;
            if (proxiedTypeMethodInfo.IsGenericMethod)
            {
                var genericArgs = proxiedTypeMethodInfo.GetGenericArguments();
                List<string> genericParameterNameList = new List<string>();
                for (var i = 0; i < genericArgs.Length; i++)
                {
                    genericParameterNameList.Add(string.Format("T{0}", i));
                }
                methodGenericParameters = method.DefineGenericParameters(genericParameterNameList.ToArray());
                for (var i = 0; i < genericArgs.Length; i++)
                {
                    methodGenericParameters[i].SetGenericParameterAttributes(genericArgs[i].GenericParameterAttributes);
                    methodGenericParameters[i].SetInterfaceConstraints(genericArgs[i].GetInterfaces());
                    methodGenericParameters[i].SetBaseTypeConstraint(genericArgs[i].BaseType);
                }
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                method.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);
            }
            var methodGenerator = method.GetILGenerator();

            // 创建参数类型列表的局部变量
            methodGenerator.Ldc_I4(parameters.Length);
            methodGenerator.Emit(OpCodes.Newarr, typeof(Type));
            for (var i = 0; i < parameters.Length; i++)
            {
                methodGenerator.Emit(OpCodes.Dup);
                methodGenerator.Ldc_I4(i);
                var unwrappedParameterType = parameters[i].ParameterType;
                if (parameters[i].ParameterType.IsByRef)
                {
                    unwrappedParameterType = parameters[i].ParameterType.GetElementType();
                }
                methodGenerator.Emit(OpCodes.Ldtoken, unwrappedParameterType);
                methodGenerator.Emit(OpCodes.Stelem_Ref);
            }
            var argumentTypeArrayField = _typeGeneratorInfo.Builder.DefineField($"_{proxiedTypeMethodInfo.Name}_ArgumentTypes", typeof(Type[]), FieldAttributes.Private | FieldAttributes.Static);
            methodGenerator.Emit(OpCodes.Stsfld, argumentTypeArrayField);

            // 创建泛型参数类型列表的局部变量
            if (proxiedTypeMethodInfo.IsGenericMethod)
            {
                methodGenerator.Ldc_I4(methodGenericParameters.Length);
                methodGenerator.Emit(OpCodes.Newarr, typeof(Type));
                for (var i = 0; i < methodGenericParameters.Length; i++)
                {
                    methodGenerator.Emit(OpCodes.Dup);
                    methodGenerator.Ldc_I4(i);
                    methodGenerator.Emit(OpCodes.Ldtoken, methodGenericParameters[i]);
                    methodGenerator.Emit(OpCodes.Stelem_Ref);
                }
            }
            else
            {
                methodGenerator.Emit(OpCodes.Ldnull);
            }
            var genericArgumentTypeArrayField = _typeGeneratorInfo.Builder.DefineField($"_{proxiedTypeMethodInfo.Name}_GenericArgumentTypes", typeof(Type[]), FieldAttributes.Private | FieldAttributes.Static);
            methodGenerator.Emit(OpCodes.Stsfld, genericArgumentTypeArrayField);

            // 创建参数列表的局部变量
            methodGenerator.Ldc_I4(parameters.Length);
            methodGenerator.Emit(OpCodes.Newarr, typeof(object));
            for (var i = 0; i < parameters.Length; i++)
            {
                methodGenerator.Emit(OpCodes.Dup);
                methodGenerator.Ldc_I4(i);
                methodGenerator.Ldarg(i + 1);
                var unwrappedParameterType = parameters[i].ParameterType;
                if (parameters[i].ParameterType.IsByRef)
                {
                    unwrappedParameterType = parameters[i].ParameterType.GetElementType();
                    methodGenerator.Emit(OpCodes.Ldind_Ref);
                }
                if (unwrappedParameterType.IsValueType)
                {
                    methodGenerator.Emit(OpCodes.Box, unwrappedParameterType);
                }
                methodGenerator.Emit(OpCodes.Stelem_Ref);
            }
            var argumentArrayVar = methodGenerator.DeclareLocal(typeof(object[]));
            methodGenerator.Emit(OpCodes.Stloc, argumentArrayVar);

            #region 创建 MethodInvocation

            methodGenerator.Emit(OpCodes.Ldsfld, interceptorsField);// 拦截器
            methodGenerator.Emit(OpCodes.Ldstr, proxiedTypeMethodInfo.Name);// 方法名
            methodGenerator.Emit(OpCodes.Ldsfld, argumentTypeArrayField);// 参数类型列表
            methodGenerator.Emit(OpCodes.Ldsfld, genericArgumentTypeArrayField);// 泛型参数类型列表
            methodGenerator.Emit(OpCodes.Ldtoken, proxiedTypeMethodInfo.ReturnType);// 返回类型            

            // 目标对象
            methodGenerator.Ldarg(0);
            methodGenerator.Emit(OpCodes.Ldfld, proxiedObjField);

            // 目标方法回调
            methodGenerator.Ldarg(0);
            if (proxiedTypeMethodInfo.IsGenericMethod)
            {
                methodGenerator.Emit(OpCodes.Ldftn, methodFunc.MakeGenericMethod(methodGenericParameters));
            }
            else
            {
                methodGenerator.Emit(OpCodes.Ldftn, methodFunc);
            }
            methodGenerator.Emit(OpCodes.Newobj, typeof(Func<object[], object>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));

            methodGenerator.Ldarg(0);// 代理对象
            methodGenerator.Emit(OpCodes.Ldloc, argumentArrayVar);// 参数列表

            methodGenerator.Emit(OpCodes.Newobj, typeof(MethodInvocation).GetConstructor(new Type[] { typeof(IInterceptor[]), typeof(string), typeof(Type[]), typeof(Type[]), typeof(Type), typeof(object), typeof(Func<object[], object>), typeof(object), typeof(object[]) }));

            var invocationVar = methodGenerator.DeclareLocal(typeof(IInvocation));
            methodGenerator.Emit(OpCodes.Stloc, invocationVar);

            #endregion

            // invocation.Proceed
            methodGenerator.Emit(OpCodes.Ldloc, invocationVar);
            methodGenerator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetMethod(nameof(IInvocation.Proceed)));

            // 验证目标方法是否被调用
            var invocatedLabel = methodGenerator.DefineLabel();
            methodGenerator.Emit(OpCodes.Ldloc, invocationVar);
            methodGenerator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetProperty(nameof(IInvocation.IsInvocationTargetInvocated)).GetMethod);
            methodGenerator.Emit(OpCodes.Brtrue_S, invocatedLabel);

            methodGenerator.Emit(OpCodes.Ldloc, invocationVar);
            methodGenerator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetProperty(nameof(IInvocation.LastProceedInterceptorType)).GetMethod);
            methodGenerator.Emit(OpCodes.Newobj, typeof(InvocationNotProceedException).GetConstructor(new Type[] { typeof(Type) }));
            methodGenerator.Emit(OpCodes.Throw, typeof(InvocationNotProceedException));
            
            methodGenerator.MarkLabel(invocatedLabel);
            
            // ref/out 参数赋值
            for (var i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].ParameterType.IsByRef)
                {
                    continue;
                }
                var unwrappedParameterType = parameters[i].ParameterType.GetElementType();
                methodGenerator.Ldarg(i + 1);
                methodGenerator.Emit(OpCodes.Ldloc, argumentArrayVar);
                methodGenerator.Ldc_I4(i);
                methodGenerator.Emit(OpCodes.Ldelem_Ref);
                if (unwrappedParameterType.IsValueType)
                {
                    methodGenerator.Emit(OpCodes.Unbox_Any, unwrappedParameterType);
                }
                methodGenerator.Emit(OpCodes.Stind_Ref);
            }

            // 返回
            if (proxiedTypeMethodInfo.ReturnType != typeof(void))
            {
                methodGenerator.Emit(OpCodes.Ldloc, invocationVar);
                methodGenerator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetProperty(nameof(IInvocation.ReturnValue)).GetMethod);
                if (proxiedTypeMethodInfo.ReturnType.IsValueType)
                {
                    methodGenerator.Emit(OpCodes.Unbox_Any, proxiedTypeMethodInfo.ReturnType);
                }
                else
                {
                    methodGenerator.Emit(OpCodes.Castclass, proxiedTypeMethodInfo.ReturnType);
                }
            }
            methodGenerator.Emit(OpCodes.Ret);

            #endregion
        }
    }
}
