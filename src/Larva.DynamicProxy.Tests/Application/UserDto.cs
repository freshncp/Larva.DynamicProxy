namespace Larva.DynamicProxy.Tests.Application
{
    public class UserDto
    {
        public string RealName { get; set; }

        public override string ToString()
        {
            return $"UserDto:RealName={RealName}";
        }
    }
}