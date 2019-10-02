using System.Threading.Tasks;

namespace DynamicProxyTests.Repositories
{
    public interface IUserLoginRepository
    {
        bool Validate(string userName, string password);

        Task<bool> ValidateAsync(string userName, string password);
    }
}