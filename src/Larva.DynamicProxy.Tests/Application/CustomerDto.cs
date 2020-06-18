namespace Larva.DynamicProxy.Tests.Application
{
    public class CustomerDto : UserDto
    {
        public override string ToString()
        {
            return $"CustomerDto:RealName={RealName}";
        }
    }
}