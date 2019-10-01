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
                    typeof(UserLoginCounterInterceptor)
                });
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);
            userLoginService.Login("jack", "123456");
            userLoginService.Login("rose", "123456");
        }

        [Fact]
        public void TestCreateProxy()
        {
            var userLoginService = (IUserLoginService)Larva.DynamicProxy.DynamicProxyFactory.CreateProxy(
                typeof(IUserLoginService),
                new UserLoginService(new UserLoginRepository()),
                new Type[] {
                    typeof(UserLoginCounterInterceptor)
                });
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);
            userLoginService.Login("jack", "123456");
            userLoginService.Login("rose", "123456");
        }

        [Fact]
        public void TestCreateProxyType()
        {
            var userLoginServiceType = Larva.DynamicProxy.DynamicProxyFactory.CreateProxyType(
                typeof(IUserLoginService),
                typeof(UserLoginService),
                new Type[] {
                    typeof(UserLoginCounterInterceptor)
                });
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByNewObj", userLoginServiceType.Name);
            var userLoginService = (IUserLoginService)Activator.CreateInstance(
                userLoginServiceType,
                new object[] {
                    new UserLoginRepository()
                });
            userLoginService.Login("jack", "123456");
            userLoginService.Login("rose", "123456");
        }

        [Fact]
        public void TestCreateProxyTypeWhenNoPublicConstructor()
        {
            Assert.Throws<Larva.DynamicProxy.InvalidProxiedTypeException>(() => {
                var userLoginServiceType = Larva.DynamicProxy.DynamicProxyFactory.CreateProxyType(
                    typeof(IUserLoginService),
                    typeof(AnotherUserLoginService),
                    new Type[] {
                        typeof(UserLoginCounterInterceptor)
                    });
            });
        }
    }
}