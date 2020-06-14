using System;
using System.Threading.Tasks;

namespace Larva.DynamicProxy.Tests.Repositories
{
    public class UserLoginRepository : IUserLoginRepository
    {
        public bool Validate(string userName, string password, int sault)
        {
            //TODO: validate
            Console.WriteLine($"userName: {userName}, password: {password}, sault: {sault}, Validate: {true}");
            return true;
        }

        public async Task<bool> ValidateAsync(string userName, string password)
        {
            //TODO: validate
            await Task.Delay(1000);
            Console.WriteLine($"userName: {userName}, password: {password}, validateAsync: {true}");
            return true;
        }
    }
}