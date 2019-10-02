using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Larva.DynamicProxy.Emitters
{
    public sealed class ProxyTypeGenerator : IProxyTypeGeneratorInfo
    {
        private Type _proxiedInterfaceType;
        private Type _cachedType;

        public ProxyTypeGenerator(ModuleBuilder moduleBuilder, Type proxiedInterfaceType, Type proxiedType, ProxyTypeGenerateWay way)
        {
            if (!proxiedInterfaceType.GetTypeInfo().IsInterface)
            {
                throw new InvalidProxiedInterfaceTypeException($"Unable to cast object of type '{proxiedType}' to type '{proxiedInterfaceType}");
            }
            if (!proxiedInterfaceType.GetTypeInfo().IsAssignableFrom(proxiedType))
            {
                throw new InvalidCastException($"Unable to cast object of type '{proxiedType}' to type '{proxiedInterfaceType}");
            }
            if (way == ProxyTypeGenerateWay.ByNewObj
                && proxiedType.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance).Length == 0)
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
            InterceptorTypesField = Builder.DefineField(Consts.INTERCEPTOR_TYPES_FIELD_NAME, typeof(Type[]), FieldAttributes.Public | FieldAttributes.Static);
        }

        public Type ProxiedType { get; private set; }

        public TypeBuilder Builder { get; private set; }

        public ProxyTypeGenerateWay Way { get; private set; }

        public FieldBuilder ProxiedObjField { get; private set; }

        public FieldBuilder InterceptorTypesField { get; private set; }

        public Type Generate()
        {
            if (_cachedType == null)
            {
                // constructor
                if (Way == ProxyTypeGenerateWay.ByNewObj)
                {
                    var cctors = ProxiedType.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance);
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
                foreach (var interfaceType in _proxiedInterfaceType.GetTypeInfo().GetInterfaces())
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
                if (array1[i].GetTypeInfo().IsGenericType && array2[i].GetTypeInfo().IsGenericType)
                {
                    if (!array1[i].GetGenericTypeDefinition().Equals(array2[i].GetGenericTypeDefinition()))
                        return false;
                }
                else if (!array1[i].Equals(array2[i]))
                {
                    return false;
                }
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
            foreach (var interfaceEventInfo in interfaceType.GetTypeInfo().GetEvents())
            {
                if (interfaceEventInfo.AddMethod != null)
                {
                    eventMethodList.Add(interfaceEventInfo.AddMethod);
                }
                if (interfaceEventInfo.RemoveMethod != null)
                {
                    eventMethodList.Add(interfaceEventInfo.RemoveMethod);
                }
                new ProxyEventEmitter(this).Emit(proxiedType.GetTypeInfo().GetEvent(interfaceEventInfo.Name));
            }
            return eventMethodList;
        }

        private List<MethodInfo> AddProperties(Type interfaceType, Type proxiedType)
        {
            var propertyMethodList = new List<MethodInfo>();
            foreach (var interfacePropertyInfo in interfaceType.GetTypeInfo().GetProperties())
            {
                if (interfacePropertyInfo.CanRead)
                {
                    propertyMethodList.Add(interfacePropertyInfo.GetMethod);
                }
                if (interfacePropertyInfo.CanWrite)
                {
                    propertyMethodList.Add(interfacePropertyInfo.SetMethod);
                }
                new ProxyPropertyEmitter(this).Emit(proxiedType.GetTypeInfo().GetProperty(interfacePropertyInfo.Name, interfacePropertyInfo.PropertyType, interfacePropertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray()));
            }
            return propertyMethodList;
        }

        private void AddMethods(Type interfaceType, Type proxiedType, IList<MethodInfo> eventMethodList, IList<MethodInfo> propertyMethodList)
        {
            foreach (var interfaceMethodInfo in interfaceType.GetTypeInfo().GetMethods())
            {
                if (!propertyMethodList.Contains(interfaceMethodInfo)
                    && !eventMethodList.Contains(interfaceMethodInfo))
                {
                    var methodInfo = proxiedType.GetTypeInfo().GetMethods().FirstOrDefault(m => IsMethodSignutureEqual(m, interfaceMethodInfo));
                    if (methodInfo == null)
                    {
                        var baseType = proxiedType.GetTypeInfo().BaseType;
                        while (baseType != null)
                        {
                            methodInfo = baseType.GetTypeInfo().GetMethods().FirstOrDefault(m => IsMethodSignutureEqual(m, interfaceMethodInfo));
                            if (methodInfo != null) break;
                            baseType = baseType.GetTypeInfo().BaseType;
                        }
                    }
                    new ProxyMethodEmitter(this).Emit(methodInfo);
                }
            }
        }
    }
}