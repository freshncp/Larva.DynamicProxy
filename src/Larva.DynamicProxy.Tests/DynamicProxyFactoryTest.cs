using System;
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
                new IInterceptor[] {
                    new UserLoginCounterInterceptor(),
                    new PerformanceCounterInterceptor()
                });
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);

            int retryCount = 1;
            var result1 = userLoginService.Login("jack", "123456", 996, out bool accountExists, ref retryCount, out UserDto userDto);
            Assert.True(result1);
            Assert.True(accountExists);
            Assert.Equal(2, retryCount);
            Assert.Equal("jack", userDto.RealName);
            Assert.Equal("jack", userLoginService.UserName);
            Assert.Equal(996, userLoginService.Sault);

            var result2 = userLoginService.LoginAsync("rose", "123456")
                .ConfigureAwait(false).GetAwaiter().GetResult();
            Assert.True(result2);
        }

        [Fact]
        public void TestCreateProxy()
        {
            var userLoginService = (IUserLoginService)Larva.DynamicProxy.DynamicProxyFactory.CreateProxy(
                typeof(IUserLoginService),
                new UserLoginService(new UserLoginRepository()),
                new IInterceptor[] {
                    new UserLoginCounterInterceptor(),
                    new PerformanceCounterInterceptor()
                });
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);
            int retryCount = 1;
            userLoginService.Login("jack", "123456", 996, out bool accountExists, ref retryCount, out UserDto userDto);
            userLoginService.LoginAsync("rose", "123456")
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Fact]
        public void TestCreateProxyType()
        {
            var userLoginServiceType = Larva.DynamicProxy.DynamicProxyFactory.CreateProxyType(
                typeof(IUserLoginService),
                typeof(UserLoginService),
                new IInterceptor[] {
                    new UserLoginCounterInterceptor(),
                    new PerformanceCounterInterceptor()
                });
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByNewObj", userLoginServiceType.Name);
            var userLoginService = (IUserLoginService)Activator.CreateInstance(
                userLoginServiceType,
                new object[] {
                    new UserLoginRepository()
                });
            int retryCount = 1;
            userLoginService.Login("jack", "123456", 996, out bool accountExists, ref retryCount, out UserDto userDto);
            userLoginService.LoginAsync("rose", "123456")
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Fact]
        public void TestCreateProxyTypeWhenNoPublicConstructor()
        {
            Assert.Throws<Larva.DynamicProxy.InvalidProxiedTypeException>(() =>
            {
                var userLoginServiceType = Larva.DynamicProxy.DynamicProxyFactory.CreateProxyType(
                    typeof(IUserLoginService),
                    typeof(AnotherUserLoginService),
                    new IInterceptor[] {
                        new UserLoginCounterInterceptor(),
                        new PerformanceCounterInterceptor()
                });
            });
        }
    }
}