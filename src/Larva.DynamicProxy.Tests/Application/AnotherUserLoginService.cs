using System.Threading.Tasks;

namespace Larva.DynamicProxy.Tests.Application
{

    public class AnotherUserLoginService : IUserLoginService
    {
        private AnotherUserLoginService()
        {
        }

        public bool Login(string userName, string password, int sault, out bool accountExists, ref int retryCount, out UserDto userDto)
        {
            UserName = userName;
            Sault = sault;
            accountExists = true;
            ++retryCount;
            userDto = null;
            return true;
        }

        public Task<bool> LoginAsync(string userName, string password)
        {
            UserName = userName;
            return Task.FromResult(false);
        }

        public T ActAs<T>(T user) where T : UserDto, new()
        {
            return user;
        }

        public string UserName { get; private set; }

        public int Sault { get; set; }
    }
}
