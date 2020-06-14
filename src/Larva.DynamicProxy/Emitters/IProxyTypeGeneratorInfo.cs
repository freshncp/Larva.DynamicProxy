using System;
using System.Reflection.Emit;

namespace Larva.DynamicProxy.Emitters
{
    /// <summary>
    /// 代理类生成信息
    /// </summary>
    public interface IProxyTypeGeneratorInfo
    {
        /// <summary>
        /// 代理类
        /// </summary>
        Type ProxiedType { get; }

        /// <summary>
        /// 类型构建器
        /// </summary>
        TypeBuilder Builder { get; }

        /// <summary>
        /// 代理类生成方式
        /// </summary>
        ProxyTypeGenerateWay Way { get; }

        /// <summary>
        /// 目标对象 字段
        /// </summary>
        FieldBuilder ProxiedObjField { get; }

        /// <summary>
        /// 拦截器 字段
        /// </summary>
        FieldBuilder InterceptorsField { get; }

        /// <summary>
        /// 生成
        /// </summary>
        Type Generate();
    }
}