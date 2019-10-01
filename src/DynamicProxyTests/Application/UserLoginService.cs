namespace DynamicProxyTests.Application
{
    public class UserLoginService : IUserLoginService
    {
        public bool Login(string userName, string password)
        {
            //TODO: Validate
            return true;
        }
    }
}
