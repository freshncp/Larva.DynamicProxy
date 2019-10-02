using System.Threading.Tasks;

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

        public Task<bool> LoginAsync(string userName, string password)
        {
            return Task.FromResult(false);
        }
    }
}
