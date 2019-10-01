# Larva.DynamicProxy
dotnet 动态代理类，用于AOP。可以结合IoC框架。此动态代理仅支持通过实现接口来创建代理类。

- 基于对象，返回指定接口的代理类对象，此代理类引用原始对象

- 基于类型，返回值ID难过接口的代理类，此代理类拥有原始类Public的构造函数


## 安装Nuget包

```
Install-Package Larva.DynamicProxy
```

## 调用示例

```csharp
public interface IUserLoginService
{
    bool Login(string userName, string password);
}

public class UserLoginService : IUserLoginService
{
    public bool Login(string userName, string password)
    {
        //TODO: Validate
        return true;
    }
}

// 定义Interceptor，用于拦截Method、Property
public class UserLoginCounterInterceptor : Larva.DynamicProxy.StandardInterceptor
{
    private static ConcurrentDictionary<string, long> _counter = new ConcurrentDictionary<string, long>();

    // 执行前
    protected override void PreProceed(Larva.DynamicProxy.IInvocation invocation)
    {
        base.PreProceed(invocation);
    }

    // 执行后
    protected override void PostProceed(Larva.DynamicProxy.IInvocation invocation)
    {
        if (invocation.InvocationTarget is IUserLoginService
            && invocation.MethodInvocationTarget.Name == nameof(IUserLoginService.Login))
        {
            var userName = (string)invocation.Arguments[0];
            _counter.TryAdd(userName, 0);
            _counter.AddOrUpdate(userName, 0, (key, originVal) => System.Threading.Interlocked.Increment(ref originVal));
            Console.WriteLine($"{userName} has login {_counter[userName]} times");
        }
        base.PostProceed(invocation);
    }
}

// 使用泛型参数，基于对象创建代理对象
var userLoginService = Larva.DynamicProxy.DynamicProxyFactory.CreateProxy<IUserLoginService>(new UserLoginService(),
    new System.Type[] {
        typeof(UserLoginCounterInterceptor)
    });
userLoginService.Login("jack", "123456");
userLoginService.Login("rose", "123456");

// 基于对象创建代理对象
var userLoginService = (IUserLoginService)Larva.DynamicProxy.DynamicProxyFactory.CreateProxy(typeof(IUserLoginService), new UserLoginService(),
    new System.Type[] {
        typeof(UserLoginCounterInterceptor)
    });
Assert.Equal($"{typeof(UserLoginService).Name}__DynamicProxyByInstance", userLoginService.GetType().Name);
userLoginService.Login("jack", "123456");
userLoginService.Login("rose", "123456");

// 基于类型创建代理类，代理类拥有和原始类相同的Public的构造函数
var userLoginServiceType = Larva.DynamicProxy.DynamicProxyFactory.CreateProxyType(typeof(IUserLoginService), typeof(UserLoginService),
    new System.Type[] {
        typeof(UserLoginCounterInterceptor)
    });
var userLoginService = (IUserLoginService)Activator.CreateInstance(userLoginServiceType);
userLoginService.Login("jack", "123456");
userLoginService.Login("rose", "123456");
```