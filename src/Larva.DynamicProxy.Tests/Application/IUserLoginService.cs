using System.Threading.Tasks;

namespace Larva.DynamicProxy.Tests.Application
{
    public interface IUserLoginService
    {
        bool Login(string userName, string password, int sault, out bool accountExists, ref int retryCount, out UserDto userDto);

        Task<bool> LoginAsync(string userName, string password);

        T ActAs<T>(T user) where T : UserDto, new();

        string UserName { get; }

        int Sault { get; set; }
    }
}
