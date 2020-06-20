using Larva.DynamicProxy.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Larva.DynamicProxy.Emitters
{
    /// <summary>
    /// 代理类生成器
    /// </summary>
    public sealed class ProxyTypeGenerator : IProxyTypeGeneratorInfo
    {
        private Type _proxiedInterfaceType;
        private Type _cachedType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moduleBuilder"></param>
        /// <param name="proxiedInterfaceType"></param>
        /// <param name="proxiedType"></param>
        /// <param name="way"></param>
        public ProxyTypeGenerator(ModuleBuilder moduleBuilder, Type proxiedInterfaceType, Type proxiedType, ProxyTypeGenerateWay way)
        {
            if (!proxiedInterfaceType.IsInterface)
            {
                throw new InvalidProxiedInterfaceTypeException($"Unable to cast object of type '{proxiedType}' to type '{proxiedInterfaceType}");
            }
            if (!proxiedInterfaceType.IsAssignableFrom(proxiedType))
            {
                throw new InvalidCastException($"Unable to cast object of type '{proxiedType}' to type '{proxiedInterfaceType}");
            }
            if (way == ProxyTypeGenerateWay.ByNewObj
                && proxiedType.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance).Length == 0)
            {
                throw new InvalidProxiedTypeException($"Type {proxiedType} that no public constructor in, cann't use '{nameof(ProxyTypeGenerateWay)}.{ProxyTypeGenerateWay.ByNewObj}'");
            }
            _proxiedInterfaceType = proxiedInterfaceType;
            ProxiedType = proxiedType;
            Builder = moduleBuilder.DefineType($"{proxiedType.Name}__DynamicProxy{way}", TypeAttributes.Public | TypeAttributes.Sealed, null, new Type[]
            {
                proxiedInterfaceType
            });
            Way = way;

            ProxiedObjField = Builder.DefineField("_proxiedObj", proxiedInterfaceType, FieldAttributes.Private);
            InterceptorsField = Builder.DefineField(Consts.INTERCEPTORS_FIELD_NAME, typeof(IInterceptor[]), FieldAttributes.Public | FieldAttributes.Static);
        }

        /// <summary>
        /// 
        /// </summary>
        public Type ProxiedType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public TypeBuilder Builder { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ProxyTypeGenerateWay Way { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public FieldBuilder ProxiedObjField { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public FieldBuilder InterceptorsField { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Type Generate()
        {
            if (_cachedType == null)
            {
                // constructor
                if (Way == ProxyTypeGenerateWay.ByNewObj)
                {
                    var cctors = ProxiedType.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance);
                    foreach (var cctor in cctors)
                    {
                        new ProxyConstructorEmitter(this).Emit(cctor);
                    }
                }
                else
                {
                    new ProxyConstructorEmitter(this).Emit(null);
                }

                AddMethods(_proxiedInterfaceType, ProxiedType, AddEvents(_proxiedInterfaceType, ProxiedType), AddProperties(_proxiedInterfaceType, ProxiedType));
                foreach (var interfaceType in _proxiedInterfaceType.GetInterfaces())
                {
                    AddMethods(interfaceType, ProxiedType, AddEvents(interfaceType, ProxiedType), AddProperties(interfaceType, ProxiedType));
                }


                _cachedType = Builder.CreateTypeInfo().AsType();
            }
            return _cachedType;
        }

        private bool IsTypeArrayEqual(Type[] array1, Type[] array2)
        {
            if (array1 == null && array2 == null)
                return true;
            if (array1 == null && array2 != null)
                return false;
            if (array1 != null && array2 == null)
                return false;
            if (array1.Length != array2.Length)
                return false;
            for (var i = 0; i < array1.Length; i++)
            {
                if (!IsTypeEqual(array1[i], array2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsTypeEqual(Type type1, Type type2)
        {
            if (type1.IsGenericType && type2.IsGenericType)
            {
                if (!type1.GetGenericTypeDefinition().Equals(type2.GetGenericTypeDefinition()))
                    return false;
            }
            else if (type1.IsGenericParameter && type2.IsGenericParameter)
            {
                if (type1.GenericParameterPosition != type2.GenericParameterPosition)
                    return false;
            }
            else if ((type1.IsByRef && type2.IsByRef)
                || (type1.IsArray && type2.IsArray))
            {
                return IsTypeEqual(type1.GetElementType(), type2.GetElementType());
            }
            else if (!type1.Equals(type2))
            {
                return false;
            }
            return true;
        }

        private bool IsMethodSignutureEqual(MethodInfo method1, MethodInfo method2)
        {
            return method1.Name == method2.Name
                && method1.GetGenericArguments().Length == method2.GetGenericArguments().Length
                && IsTypeArrayEqual(method1.GetParameters().Select(m => m.ParameterType).ToArray(), method2.GetParameters().Select(m => m.ParameterType).ToArray());
        }

        private List<MethodInfo> AddEvents(Type interfaceType, Type proxiedType)
        {
            var eventMethodList = new List<MethodInfo>();
            foreach (var interfaceEventInfo in interfaceType.GetEvents())
            {
                if (interfaceEventInfo.AddMethod != null)
                {
                    eventMethodList.Add(interfaceEventInfo.AddMethod);
                }
                if (interfaceEventInfo.RemoveMethod != null)
                {
                    eventMethodList.Add(interfaceEventInfo.RemoveMethod);
                }
                new ProxyEventEmitter(this).Emit(proxiedType.GetEvent(interfaceEventInfo.Name));
            }
            return eventMethodList;
        }

        private List<MethodInfo> AddProperties(Type interfaceType, Type proxiedType)
        {
            var propertyMethodList = new List<MethodInfo>();
            foreach (var interfacePropertyInfo in interfaceType.GetProperties())
            {
                if (interfacePropertyInfo.CanRead)
                {
                    propertyMethodList.Add(interfacePropertyInfo.GetMethod);
                }
                if (interfacePropertyInfo.CanWrite)
                {
                    propertyMethodList.Add(interfacePropertyInfo.SetMethod);
                }
#if DEBUG
                Console.WriteLine($"Property: {interfacePropertyInfo.PropertyType.Name} {interfacePropertyInfo.Name}");
#endif
                new ProxyPropertyEmitter(this).Emit(proxiedType.GetProperty(interfacePropertyInfo.Name, interfacePropertyInfo.PropertyType, interfacePropertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray()));
            }
            return propertyMethodList;
        }

        private void AddMethods(Type interfaceType, Type proxiedType, IList<MethodInfo> eventMethodList, IList<MethodInfo> propertyMethodList)
        {
            foreach (var interfaceMethodInfo in interfaceType.GetMethods())
            {
                if (!propertyMethodList.Contains(interfaceMethodInfo)
                    && !eventMethodList.Contains(interfaceMethodInfo))
                {
                    var methodInfo = proxiedType.GetMethods().FirstOrDefault(m => IsMethodSignutureEqual(m, interfaceMethodInfo));
                    if (methodInfo == null)
                    {
                        var baseType = proxiedType.BaseType;
                        while (baseType != null)
                        {
                            methodInfo = baseType.GetMethods().FirstOrDefault(m => IsMethodSignutureEqual(m, interfaceMethodInfo));
                            if (methodInfo != null) break;
                            baseType = baseType.BaseType;
                        }
                    }
#if DEBUG
                    Console.WriteLine($"Method: {methodInfo.ReturnType.Name} {methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(s => $"{s.ParameterType.Name} {s.Name}"))})");

#endif
                    new ProxyMethodEmitter(this).Emit(methodInfo);
                }
            }
        }
    }
}
