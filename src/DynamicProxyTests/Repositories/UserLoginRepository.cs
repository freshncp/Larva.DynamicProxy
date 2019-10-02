using System;
using System.Threading.Tasks;

namespace DynamicProxyTests.Repositories
{
    public class UserLoginRepository : IUserLoginRepository
    {
        public bool Validate(string userName, string password)
        {
            //TODO: validate
            // throw new NotSupportedException(nameof(Validate));
            return true;
        }

        public async Task<bool> ValidateAsync(string userName, string password)
        {
            //TODO: validate
            await Task.Delay(1000);
            // throw new NotSupportedException(nameof(ValidateAsync));
            Console.WriteLine($"validate: {true}");
            return true;
        }
    }
}