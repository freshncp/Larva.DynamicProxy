using System;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicProxyTests.Repositories
{
    public class UserLoginRepository : IUserLoginRepository
    {
        public bool Validate(string userName, string password)
        {
            //TODO: validate
            return true;
        }

        public async Task<bool> ValidateAsync(string userName, string password)
        {
            //TODO: validate
            await Task.Delay(1000);
            Console.WriteLine($"validate: {true}");
            return true;
        }
    }
}