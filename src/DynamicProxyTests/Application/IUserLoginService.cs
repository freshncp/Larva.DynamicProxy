using System.Threading.Tasks;

namespace DynamicProxyTests.Application
{
    public interface IUserLoginService
    {
        bool Login(string userName, string password, out bool accountExists, ref int retryCount, out UserDto userDto);

        Task<bool> LoginAsync(string userName, string password);
    }
}
