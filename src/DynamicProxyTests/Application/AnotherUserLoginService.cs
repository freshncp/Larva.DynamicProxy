namespace DynamicProxyTests.Application
{

    public class AnotherUserLoginService : IUserLoginService
    {
        private AnotherUserLoginService()
        {
        }

        public bool Login(string userName, string password)
        {
            return true;
        }
    }
}
