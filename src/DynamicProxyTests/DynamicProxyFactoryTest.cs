using System;
using DynamicProxyTests.Application;
using DynamicProxyTests.Interceptors;
using Xunit;

namespace DynamicProxyTests
{
    public class DynamicProxyFactoryTest
    {
        [Fact]
        public void TestCreateProxyT()
        {
            var userLoginService = Larva.DynamicProxy.DynamicProxyFactory.CreateProxy<IUserLoginService>(new UserLoginService(),
                new System.Type[] {
                    typeof(UserLoginCounterInterceptor)
                });
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);
            userLoginService.Login("jack", "123456");
            userLoginService.Login("rose", "123456");
        }

        [Fact]
        public void TestCreateProxy()
        {
            var userLoginService = (IUserLoginService)Larva.DynamicProxy.DynamicProxyFactory.CreateProxy(typeof(IUserLoginService), new UserLoginService(),
                new System.Type[] {
                    typeof(UserLoginCounterInterceptor)
                });
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);
            userLoginService.Login("jack", "123456");
            userLoginService.Login("rose", "123456");
        }

        [Fact]
        public void TestCreateProxyType()
        {
            var userLoginServiceType = Larva.DynamicProxy.DynamicProxyFactory.CreateProxyType(typeof(IUserLoginService), typeof(UserLoginService),
                new System.Type[] {
                    typeof(UserLoginCounterInterceptor)
                });
            Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByNewObj", userLoginServiceType.Name);
            var userLoginService = (IUserLoginService)Activator.CreateInstance(userLoginServiceType);
            userLoginService.Login("jack", "123456");
            userLoginService.Login("rose", "123456");
        }

        [Fact]
        public void TestCreateProxyTypeWhenNoPublicConstructor()
        {
            Assert.Throws<Larva.DynamicProxy.InvalidProxiedTypeException>(() => {
                var userLoginServiceType = Larva.DynamicProxy.DynamicProxyFactory.CreateProxyType(typeof(IUserLoginService), typeof(AnotherUserLoginService),
                new System.Type[] {
                    typeof(UserLoginCounterInterceptor)
                });
            });
        }
    }
}