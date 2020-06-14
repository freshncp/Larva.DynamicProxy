namespace Larva.DynamicProxy
{
    /// <summary>
    /// 拦截器
    /// </summary>
    public interface IInterceptor
    {
        /// <summary>
        /// 拦截
        /// </summary>
        /// <param name="invocation"></param>
        void Intercept(IInvocation invocation);
    }
}
