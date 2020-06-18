using Larva.DynamicProxy.Tests.Repositories;
using System.Threading.Tasks;

namespace Larva.DynamicProxy.Tests.Application
{
    public class UserLoginService : IUserLoginService
    {
        private IUserLoginRepository _userLoginRepository;

        public UserLoginService(IUserLoginRepository userLoginRepository)
        {
            _userLoginRepository = userLoginRepository;
        }

        public bool Login(string userName, string password, int sault, out bool accountExists, ref int retryCount, out UserDto userDto)
        {
            UserName = userName;
            Sault = sault;
            accountExists = true;
            ++retryCount;
            userDto = new UserDto { RealName = userName };
            return _userLoginRepository.Validate(userName, password, sault);
        }

        public async Task<bool> LoginAsync(string userName, string password)
        {
            UserName = userName;
            return await _userLoginRepository.ValidateAsync(userName, password);
        }

        public T ActAs<T>(T user) where T : UserDto, new()
        {
            return user;
        }

        public string UserName { get; private set; }

        public int Sault { get; set; }
    }
}
