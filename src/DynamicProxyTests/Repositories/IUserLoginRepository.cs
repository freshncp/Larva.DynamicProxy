namespace DynamicProxyTests.Repositories
{
    public interface IUserLoginRepository
    {
        bool Validate(string userName, string password);
    }
}