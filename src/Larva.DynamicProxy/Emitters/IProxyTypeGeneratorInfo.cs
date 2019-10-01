using System;
using System.Reflection.Emit;

namespace Larva.DynamicProxy.Emitters
{
    public interface IProxyTypeGeneratorInfo
    {
        Type ProxiedType { get; }

        TypeBuilder Builder { get; }

        ProxyTypeGenerateWay Way { get; }

        FieldBuilder ProxiedObjField { get; }

        FieldBuilder InterceptorTypesField { get; }

        Type Generate();
    }
}