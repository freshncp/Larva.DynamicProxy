using DynamicProxyTests.Repositories;
using System.Threading.Tasks;

namespace DynamicProxyTests.Application
{
    public class UserLoginService : IUserLoginService
    {
        private IUserLoginRepository _userLoginRepository;

        public UserLoginService(IUserLoginRepository userLoginRepository)
        {
            _userLoginRepository = userLoginRepository;
        }

        public bool Login(string userName, string password)
        {
            return _userLoginRepository.Validate(userName, password);
        }

        public async Task<bool> LoginAsync(string userName, string password)
        {
            return await _userLoginRepository.ValidateAsync(userName, password);
        }
    }
}
