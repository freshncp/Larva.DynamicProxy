using System.Threading.Tasks;

namespace DynamicProxyTests.Application
{
    public interface IUserLoginService
    {
        bool Login(string userName, string password);

        Task<bool> LoginAsync(string userName, string password);
    }
}
