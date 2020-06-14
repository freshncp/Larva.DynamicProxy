using System.Threading.Tasks;

namespace Larva.DynamicProxy.Tests.Repositories
{
    public interface IUserLoginRepository
    {
        bool Validate(string userName, string password, int sault);

        Task<bool> ValidateAsync(string userName, string password);
    }
}