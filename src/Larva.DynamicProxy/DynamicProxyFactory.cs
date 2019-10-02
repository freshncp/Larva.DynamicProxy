using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Larva.DynamicProxy.Emitters;

namespace Larva.DynamicProxy
{
    public sealed class DynamicProxyFactory
    {
        private static ConcurrentDictionary<ProxyTypeIdentity, ProxyTypeWrapper> _proxyTypeDics = new ConcurrentDictionary<ProxyTypeIdentity, ProxyTypeWrapper>();

        public static object CreateProxy(Type interfaceType, object proxiedObj, Type[] interceptorTypes)
        {
            var proxyType = InternalCreateProxyType(interfaceType, proxiedObj.GetType(), interceptorTypes).ProxyTypeByInstance;
            return Activator.CreateInstance(proxyType, proxiedObj);
        }

        public static TInterface CreateProxy<TInterface>(TInterface proxiedObj, Type[] interceptorTypes)
            where TInterface : class
        {
            return (TInterface)CreateProxy(typeof(TInterface), proxiedObj, interceptorTypes);
        }

        public static Type CreateProxyType(Type interfaceType, Type proxiedType, Type[] interceptorTypes)
        {
            return InternalCreateProxyType(interfaceType, proxiedType, interceptorTypes).ProxyTypeByNewObj;
        }

        internal static ProxyTypeWrapper InternalCreateProxyType(Type interfaceType, Type proxiedType, Type[] interceptorTypes)
        {
            var key = new ProxyTypeIdentity(interfaceType, proxiedType);
            var proxyType = _proxyTypeDics.AddOrUpdate(key, t =>
            {
                AssemblyName assemblyName = new AssemblyName(string.Format("{0}_{1}__DynamicAssembly", proxiedType.Name, interfaceType.Name));
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule(string.Format("{0}_{1}__DynamicModule", proxiedType.Name, interfaceType.Name));

                return new ProxyTypeWrapper
                {
                    ProxyTypeByNewObj = new ProxyTypeGenerator(moduleBuilder, interfaceType, proxiedType, ProxyTypeGenerateWay.ByNewObj).Generate(),
                    ProxyTypeByInstance = new ProxyTypeGenerator(moduleBuilder, interfaceType, proxiedType, ProxyTypeGenerateWay.ByInstance).Generate(),
                    InterceptorTypes = interceptorTypes == null ? null : interceptorTypes.Where(i => typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(i)).Distinct().ToArray()
                };
            }, (t, originVal) =>
            {
                if (interceptorTypes != null)
                {
                    interceptorTypes = interceptorTypes.Where(i => typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(i)).ToArray();
                    List<Type> interceptorTypeList = new List<Type>(interceptorTypes);
                    if (_proxyTypeDics[key].InterceptorTypes != null)
                    {
                        interceptorTypeList.AddRange(_proxyTypeDics[key].InterceptorTypes);
                    }
                    originVal.InterceptorTypes = interceptorTypeList.Distinct().ToArray();
                }
                return originVal;
            });
            proxyType.ProxyTypeByNewObj.GetTypeInfo().GetField(Consts.INTERCEPTOR_TYPES_FIELD_NAME, BindingFlags.Public | BindingFlags.Static | BindingFlags.SetField).SetValue(null, proxyType.InterceptorTypes);
            proxyType.ProxyTypeByInstance.GetTypeInfo().GetField(Consts.INTERCEPTOR_TYPES_FIELD_NAME, BindingFlags.Public | BindingFlags.Static | BindingFlags.SetField).SetValue(null, proxyType.InterceptorTypes);
            return proxyType;
        }
    }

    internal sealed class ProxyTypeWrapper
    {
        public Type ProxyTypeByNewObj { get; set; }

        public Type ProxyTypeByInstance { get; set; }

        public Type[] InterceptorTypes { get; set; }
    }

    internal struct ProxyTypeIdentity
    {
        public ProxyTypeIdentity(Type interfaceType, Type proxiedType)
        {
            InterfaceType = interfaceType;
            ProxiedType = proxiedType;
        }

        public Type InterfaceType { get; private set; }

        public Type ProxiedType { get; private set; }
    }
}