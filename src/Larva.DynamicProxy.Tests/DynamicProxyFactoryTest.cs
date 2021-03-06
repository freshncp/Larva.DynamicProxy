using System.Threading.Tasks;
using Larva.DynamicProxy.Interception;
using Larva.DynamicProxy.Tests.Application;
using Larva.DynamicProxy.Tests.Interceptors;
using Larva.DynamicProxy.Tests.Repositories;
using Xunit;

namespace Larva.DynamicProxy.Tests
{
    public class DynamicProxyFactoryTest
    {
        [Fact]
        public void TestCreateProxyT()
        {
            var userLoginService = Larva.DynamicProxy.DynamicProxyFactory.CreateProxy<IUserLoginService>(
                new UserLoginService(new UserLoginRepository()),
                new UserLoginCounterInterceptor(),
                new PerformanceCounterInterceptor());
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);
        }

        [Fact]
        public void TestCreateProxy()
        {
            var userLoginService = (IUserLoginService)Larva.DynamicProxy.DynamicProxyFactory.CreateProxy(
                typeof(IUserLoginService),
                new UserLoginService(new UserLoginRepository()),
                new UserLoginCounterInterceptor(),
                new PerformanceCounterInterceptor());
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);
        }

        [Fact]
        public void TestCreateProxyType()
        {
            var userLoginServiceType = Larva.DynamicProxy.DynamicProxyFactory.CreateProxyType(
                typeof(IUserLoginService),
                typeof(UserLoginService),
                new UserLoginCounterInterceptor(),
                new PerformanceCounterInterceptor());
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByNewObj", userLoginServiceType.Name);
        }

        [Fact]
        public void TestCreateProxyTypeWhenNoPublicConstructor()
        {
            Assert.Throws<Larva.DynamicProxy.InvalidProxiedTypeException>(() =>
            {
                var userLoginServiceType = Larva.DynamicProxy.DynamicProxyFactory.CreateProxyType(
                    typeof(IUserLoginService),
                    typeof(AnotherUserLoginService),
                    new UserLoginCounterInterceptor(),
                    new PerformanceCounterInterceptor());
            });
        }

        [Fact]
        public void TestSyncMethod()
        {
            var userLoginService = Larva.DynamicProxy.DynamicProxyFactory.CreateProxy<IUserLoginService>(
                new UserLoginService(new UserLoginRepository()),
                new UserLoginCounterInterceptor(),
                new PerformanceCounterInterceptor());
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);

            int retryCount = 1;
            var result = userLoginService.Login("jack", "123456", 996, out bool accountExists, ref retryCount, out UserDto userDto);
            Assert.True(result);
            Assert.True(accountExists);
            Assert.Equal(2, retryCount);
            Assert.Equal("jack", userDto.RealName);
            Assert.Equal("jack", userLoginService.UserName);
            Assert.Equal(996, userLoginService.Sault);
        }

        [Fact]
        public async Task TestAsyncMethod()
        {
            var userLoginService = Larva.DynamicProxy.DynamicProxyFactory.CreateProxy<IUserLoginService>(
                new UserLoginService(new UserLoginRepository()),
                new UserLoginCounterInterceptor(),
                new PerformanceCounterInterceptor());
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);

            var result = await userLoginService.LoginAsync("rose", "123456")
                .ConfigureAwait(false);
            Assert.True(result);
        }

        [Fact]
        public void TestGenericTypeMethod()
        {
            var userLoginService = Larva.DynamicProxy.DynamicProxyFactory.CreateProxy<IUserLoginService>(
                new UserLoginService(new UserLoginRepository()),
                new UserLoginCounterInterceptor(),
                new PerformanceCounterInterceptor());
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);

            var result = userLoginService.ActAs<CustomerDto>(new CustomerDto { RealName = "jerry" });
            Assert.Equal("CustomerDto:RealName=jerry", result.ToString());
        }

        [Fact]
        public void TestNotInvokeProcessFromInterceptorMustThrowInvocationNotProceedException()
        {
            var userLoginService = Larva.DynamicProxy.DynamicProxyFactory.CreateProxy<IUserLoginService>(
                new UserLoginService(new UserLoginRepository()),
                new BadInterceptor());
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);

            var ex = Assert.Throws<Larva.DynamicProxy.InvocationNotProceedException>(() =>
            {
                int retryCount = 1;
                var result = userLoginService.Login("jack", "123456", 996, out bool accountExists, ref retryCount, out UserDto userDto);
            });
            Assert.Equal($"Interceptor \"{typeof(BadInterceptor).FullName}\" not invoke method \"{nameof(IInvocation.Proceed)}\" of IInvocation.", ex.Message);
            Assert.Equal(typeof(BadInterceptor), ex.LastProceedInterceptorType);
        }
    }
}