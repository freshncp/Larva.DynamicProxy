namespace Larva.DynamicProxy
{
    /// <summary>
    /// 成员操作类型
    /// </summary>
    public enum MemberOperateTypes
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,

        /// <summary>
        /// 属性Get方法
        /// </summary>
        PropertyGet = 1,

        /// <summary>
        /// 属性Set方法
        /// </summary>
        PropertySet = 2
    }
}
