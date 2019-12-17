using System;
using DynamicProxyTests.Application;
using DynamicProxyTests.Interceptors;
using DynamicProxyTests.Repositories;
using Xunit;

namespace DynamicProxyTests
{
    public class DynamicProxyFactoryTest
    {
        [Fact]
        public void TestCreateProxyT()
        {
            var userLoginService = Larva.DynamicProxy.DynamicProxyFactory.CreateProxy<IUserLoginService>(
                new UserLoginService(new UserLoginRepository()),
                new Type[] {
                    typeof(UserLoginCounterInterceptor),
                    typeof(PerformanceCounterInterceptor)
                });
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);
            int retryCount = 1;
            userLoginService.Login("jack", "123456", out bool accountExists, ref retryCount, out UserDto userDto);
            userLoginService.LoginAsync("rose", "123456")
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Fact]
        public void TestCreateProxy()
        {
            var userLoginService = (IUserLoginService)Larva.DynamicProxy.DynamicProxyFactory.CreateProxy(
                typeof(IUserLoginService),
                new UserLoginService(new UserLoginRepository()),
                new Type[] {
                    typeof(UserLoginCounterInterceptor),
                    typeof(PerformanceCounterInterceptor)
                });
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);
            int retryCount = 1;
            userLoginService.Login("jack", "123456", out bool accountExists, ref retryCount, out UserDto userDto);
            userLoginService.LoginAsync("rose", "123456")
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Fact]
        public void TestCreateProxyType()
        {
            var userLoginServiceType = Larva.DynamicProxy.DynamicProxyFactory.CreateProxyType(
                typeof(IUserLoginService),
                typeof(UserLoginService),
                new Type[] {
                    typeof(UserLoginCounterInterceptor),
                    typeof(PerformanceCounterInterceptor)
                });
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByNewObj", userLoginServiceType.Name);
            var userLoginService = (IUserLoginService)Activator.CreateInstance(
                userLoginServiceType,
                new object[] {
                    new UserLoginRepository()
                });
            int retryCount = 1;
            userLoginService.Login("jack", "123456", out bool accountExists, ref retryCount, out UserDto userDto);
            userLoginService.LoginAsync("rose", "123456")
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Fact]
        public void TestCreateProxyTypeWhenNoPublicConstructor()
        {
            Assert.Throws<Larva.DynamicProxy.InvalidProxiedTypeException>(() => {
                var userLoginServiceType = Larva.DynamicProxy.DynamicProxyFactory.CreateProxyType(
                    typeof(IUserLoginService),
                    typeof(AnotherUserLoginService),
                    new Type[] {
                        typeof(UserLoginCounterInterceptor),
                        typeof(PerformanceCounterInterceptor)
                    });
            });
        }
    }
}