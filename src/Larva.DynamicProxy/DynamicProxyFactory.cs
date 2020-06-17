using Larva.DynamicProxy.Emitters;
using Larva.DynamicProxy.Interceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Larva.DynamicProxy
{
    /// <summary>
    /// 代理类工厂
    /// </summary>
    public sealed class DynamicProxyFactory
    {
        private static ConcurrentDictionary<ProxyTypeIdentity, ProxyTypeWrapper> _proxyTypeDics = new ConcurrentDictionary<ProxyTypeIdentity, ProxyTypeWrapper>();

        /// <summary>
        /// 创建代理
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="proxiedObj"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static object CreateProxy(Type interfaceType, object proxiedObj, params IInterceptor[] interceptors)
        {
            var proxyType = InternalCreateProxyType(interfaceType, proxiedObj.GetType(), interceptors).ProxyTypeByInstance;
            return Activator.CreateInstance(proxyType, proxiedObj);
        }

        /// <summary>
        /// 创建代理
        /// </summary>
        /// <param name="proxiedObj"></param>
        /// <param name="interceptors"></param>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public static TInterface CreateProxy<TInterface>(TInterface proxiedObj, params IInterceptor[] interceptors)
            where TInterface : class
        {
            return (TInterface)CreateProxy(typeof(TInterface), proxiedObj, interceptors);
        }

        /// <summary>
        /// 创建代理类
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="proxiedType"></param>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public static Type CreateProxyType(Type interfaceType, Type proxiedType, params IInterceptor[] interceptors)
        {
            return InternalCreateProxyType(interfaceType, proxiedType, interceptors).ProxyTypeByNewObj;
        }

        internal static ProxyTypeWrapper InternalCreateProxyType(Type interfaceType, Type proxiedType, IInterceptor[] interceptors)
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
                    Interceptors = interceptors
                };
            }, (t, originVal) =>
            {
                if (interceptors != null)
                {
                    var interceptorList = new List<IInterceptor>(interceptors);
                    if (_proxyTypeDics[key].Interceptors != null)
                    {
                        interceptorList.AddRange(_proxyTypeDics[key].Interceptors);
                    }
                    originVal.Interceptors = interceptorList.Distinct().ToArray();
                }
                return originVal;
            });
            proxyType.ProxyTypeByNewObj.GetField(Consts.INTERCEPTORS_FIELD_NAME, BindingFlags.Public | BindingFlags.Static | BindingFlags.SetField).SetValue(null, proxyType.Interceptors);
            proxyType.ProxyTypeByInstance.GetField(Consts.INTERCEPTORS_FIELD_NAME, BindingFlags.Public | BindingFlags.Static | BindingFlags.SetField).SetValue(null, proxyType.Interceptors);
            return proxyType;
        }
    }

    internal sealed class ProxyTypeWrapper
    {
        public Type ProxyTypeByNewObj { get; set; }

        public Type ProxyTypeByInstance { get; set; }

        public IInterceptor[] Interceptors { get; set; }
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