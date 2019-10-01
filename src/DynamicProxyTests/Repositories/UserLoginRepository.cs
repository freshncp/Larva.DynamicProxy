namespace DynamicProxyTests.Repositories
{
    public class UserLoginRepository : IUserLoginRepository
    {
        public bool Validate(string userName, string password)
        {
            //TODO: validate
            return true;
        }
    }
}