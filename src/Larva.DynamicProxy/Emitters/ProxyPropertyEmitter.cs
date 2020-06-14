using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Larva.DynamicProxy.Emitters
{
    /// <summary>
    /// 代理类属性 IL生成器
    /// </summary>
    public sealed class ProxyPropertyEmitter : IMemberEmitter
    {
        private IProxyTypeGeneratorInfo _typeGeneratorInfo;

        /// <summary>
        /// 代理类属性 IL生成器
        /// </summary>
        /// <param name="typeGeneratorInfo"></param>
        public ProxyPropertyEmitter(IProxyTypeGeneratorInfo typeGeneratorInfo)
        {
            _typeGeneratorInfo = typeGeneratorInfo;
        }

        /// <summary>
        /// 生成
        /// </summary>
        /// <param name="memberInfo"></param>
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

            var interceptorsField = _typeGeneratorInfo.InterceptorsField;
            var proxiedObjField = _typeGeneratorInfo.ProxiedObjField;

            if (proxiedTypePropertyInfo.CanRead)
            {
                #region 私有方法，用于生成目标方法回调

                var getMethodFunc = _typeGeneratorInfo.Builder.DefineMethod($"__get_{proxiedTypePropertyInfo.Name}__", MethodAttributes.Private, typeof(object), Type.EmptyTypes);
                var getMethodFuncGenerator = getMethodFunc.GetILGenerator();

                // 获取返回值，无返回类型的方法，返回值设为null
                getMethodFuncGenerator.Emit(OpCodes.Ldarg_0);
                getMethodFuncGenerator.Emit(OpCodes.Ldfld, proxiedObjField);

                getMethodFuncGenerator.Emit(OpCodes.Callvirt, proxiedTypePropertyInfo.GetMethod);
                var getMethodFuncReturnValueVal = getMethodFuncGenerator.DeclareLocal(typeof(object));
                if (proxiedTypePropertyInfo.PropertyType.IsValueType)
                {
                    getMethodFuncGenerator.Emit(OpCodes.Box, proxiedTypePropertyInfo.PropertyType);
                }
                getMethodFuncGenerator.Emit(OpCodes.Stloc, getMethodFuncReturnValueVal);

                getMethodFuncGenerator.Emit(OpCodes.Ldloc, getMethodFuncReturnValueVal);
                getMethodFuncGenerator.Emit(OpCodes.Ret);

                #endregion

                #region 代理方法

                var getMethod = _typeGeneratorInfo.Builder.DefineMethod("get_" + proxiedTypePropertyInfo.Name, getSetAttr, proxiedTypePropertyInfo.PropertyType, null);
                var getMethodGenerator = getMethod.GetILGenerator();

                #region 创建 MethodInvocation

                getMethodGenerator.Emit(OpCodes.Ldsfld, interceptorsField);// 拦截器
                getMethodGenerator.Emit(OpCodes.Ldstr, proxiedTypePropertyInfo.Name);// 属性名
                getMethodGenerator.Emit(OpCodes.Ldtoken, proxiedTypePropertyInfo.PropertyType);// 属性类型            

                // 目标对象
                getMethodGenerator.Emit(OpCodes.Ldarg_0);
                getMethodGenerator.Emit(OpCodes.Ldfld, proxiedObjField);

                // 目标方法回调
                getMethodGenerator.Emit(OpCodes.Ldarg_0);
                getMethodGenerator.Emit(OpCodes.Ldftn, getMethodFunc);
                getMethodGenerator.Emit(OpCodes.Newobj, typeof(Func<object>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));

                getMethodGenerator.Emit(OpCodes.Ldarg_0);// 代理对象

                getMethodGenerator.Emit(OpCodes.Newobj, typeof(PropertyGetInvocation).GetConstructor(new Type[] { typeof(IInterceptor[]), typeof(string), typeof(Type), typeof(object), typeof(Func<object>), typeof(object) }));

                var invocationVar = getMethodGenerator.DeclareLocal(typeof(IInvocation));
                getMethodGenerator.Emit(OpCodes.Stloc, invocationVar);

                #endregion

                // invocation.Proceed
                getMethodGenerator.Emit(OpCodes.Ldloc, invocationVar);
                getMethodGenerator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetMethod(nameof(IInvocation.Proceed)));

                // 返回
                getMethodGenerator.Emit(OpCodes.Ldloc, invocationVar);
                getMethodGenerator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetProperty(nameof(IInvocation.ReturnValue)).GetMethod);
                if (proxiedTypePropertyInfo.PropertyType.IsValueType)
                {
                    getMethodGenerator.Emit(OpCodes.Unbox_Any, proxiedTypePropertyInfo.PropertyType);
                }
                else
                {
                    getMethodGenerator.Emit(OpCodes.Castclass, proxiedTypePropertyInfo.PropertyType);
                }
                getMethodGenerator.Emit(OpCodes.Ret);
                property.SetGetMethod(getMethod);

                #endregion
            }
            if (proxiedTypePropertyInfo.CanWrite)
            {
                #region 私有方法，用于生成目标方法回调

                var setMethodFunc = _typeGeneratorInfo.Builder.DefineMethod($"__set_{proxiedTypePropertyInfo.Name}__", MethodAttributes.Private, typeof(void), new Type[] { typeof(object) });
                var setMethodFuncGenerator = setMethodFunc.GetILGenerator();

                // 参数列表Copy到局部变量
                setMethodFuncGenerator.Emit(OpCodes.Ldarg_1);
                if (proxiedTypePropertyInfo.PropertyType.IsValueType)
                {
                    setMethodFuncGenerator.Emit(OpCodes.Unbox_Any, proxiedTypePropertyInfo.PropertyType);
                }
                else if (proxiedTypePropertyInfo.PropertyType != typeof(object))
                {
                    setMethodFuncGenerator.Emit(OpCodes.Castclass, proxiedTypePropertyInfo.PropertyType);
                }
                var methodFuncArgumentVar = setMethodFuncGenerator.DeclareLocal(proxiedTypePropertyInfo.PropertyType);
                setMethodFuncGenerator.Emit(OpCodes.Stloc, methodFuncArgumentVar);

                // 获取返回值，无返回类型的方法，返回值设为null
                setMethodFuncGenerator.Emit(OpCodes.Ldarg_0);
                setMethodFuncGenerator.Emit(OpCodes.Ldfld, proxiedObjField);
                setMethodFuncGenerator.Emit(OpCodes.Ldloc, methodFuncArgumentVar);
                setMethodFuncGenerator.Emit(OpCodes.Callvirt, proxiedTypePropertyInfo.SetMethod);
                setMethodFuncGenerator.Emit(OpCodes.Ret);

                #endregion

                #region 代理方法

                var setMethod = _typeGeneratorInfo.Builder.DefineMethod("set_" + proxiedTypePropertyInfo.Name, getSetAttr, null, new Type[] { proxiedTypePropertyInfo.PropertyType });
                var methodParameter = proxiedTypePropertyInfo.SetMethod.GetParameters()[0];
                setMethod.DefineParameter(1, methodParameter.Attributes, methodParameter.Name);
                var setMethodGenerator = setMethod.GetILGenerator();

                // 创建参数列表的局部变量
                setMethodGenerator.Emit(OpCodes.Ldarg_1);
                if (proxiedTypePropertyInfo.DeclaringType.IsValueType)
                {
                    setMethodGenerator.Emit(OpCodes.Box, proxiedTypePropertyInfo.DeclaringType);
                }
                var argumentVar = setMethodGenerator.DeclareLocal(typeof(object));
                setMethodGenerator.Emit(OpCodes.Stloc, argumentVar);

                #region 创建 MethodInvocation

                setMethodGenerator.Emit(OpCodes.Ldsfld, interceptorsField);// 拦截器
                setMethodGenerator.Emit(OpCodes.Ldstr, proxiedTypePropertyInfo.Name);// 属性名
                setMethodGenerator.Emit(OpCodes.Ldtoken, proxiedTypePropertyInfo.PropertyType);// 属性类型            

                // 目标对象
                setMethodGenerator.Emit(OpCodes.Ldarg_0);
                setMethodGenerator.Emit(OpCodes.Ldfld, proxiedObjField);

                // 目标方法回调
                setMethodGenerator.Emit(OpCodes.Ldarg_0);
                setMethodGenerator.Emit(OpCodes.Ldftn, setMethodFunc);
                setMethodGenerator.Emit(OpCodes.Newobj, typeof(Action<object>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));

                setMethodGenerator.Emit(OpCodes.Ldarg_0);// 代理对象
                setMethodGenerator.Emit(OpCodes.Ldloc, argumentVar);// 参数

                setMethodGenerator.Emit(OpCodes.Newobj, typeof(PropertySetInvocation).GetConstructor(new Type[] { typeof(IInterceptor[]), typeof(string), typeof(Type), typeof(object), typeof(Action<object>), typeof(object), typeof(object) }));

                var invocationVar = setMethodGenerator.DeclareLocal(typeof(IInvocation));
                setMethodGenerator.Emit(OpCodes.Stloc, invocationVar);

                #endregion

                // invocation.Proceed
                setMethodGenerator.Emit(OpCodes.Ldloc, invocationVar);
                setMethodGenerator.Emit(OpCodes.Callvirt, typeof(IInvocation).GetMethod(nameof(IInvocation.Proceed)));

                // 返回
                setMethodGenerator.Emit(OpCodes.Ret);
                property.SetSetMethod(setMethod);

                #endregion
            }
        }
    }
}
