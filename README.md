# Larva.DynamicProxy
dotnet 动态代理类，用于AOP。可以结合IoC框架。此动态代理仅支持通过实现接口来创建代理类。

- 基于对象，返回指定接口的代理类对象，此代理类引用原始对象；

- 基于类型，返回值ID难过接口的代理类，此代理类拥有原始类Public的构造函数；

- 通过实现 IInvocation 接口，并将其实现类的类型作为参数传入创建代理或代理类，即可实现AOP。


## 安装Nuget包

```
Install-Package Larva.DynamicProxy
```

## 调用示例

```csharp
public interface IUserLoginRepository
{
    bool Validate(string userName, string password);
}

public class UserLoginRepository : IUserLoginRepository
{
    public bool Validate(string userName, string password)
    {
        //TODO: validate
        return true;
    }
}

public interface IUserLoginService
{
    bool Login(string userName, string password);
}

public class UserLoginService : IUserLoginService
{
    private IUserLoginRepository _userLoginRepository;

    public UserLoginService(IUserLoginRepository userLoginRepository)
    {
        _userLoginRepository = userLoginRepository;
    }

    public bool Login(string userName, string password)
    {
        return _userLoginRepository.Validate(userName, password);
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

// 也可以直接实现接口 Larva.DynamicProxy.IInterceptor
public class ExampleInterceptor : Larva.DynamicProxy.IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        //TODO:执行前
        invocation.Proceed();
        //TODO:执行后
    }
}

// 使用泛型参数，基于对象创建代理对象
var userLoginService = Larva.DynamicProxy.DynamicProxyFactory.CreateProxy<IUserLoginService>(
    new UserLoginService(new UserLoginRepository()),
    new System.Type[] {
        typeof(UserLoginCounterInterceptor)
    });
userLoginService.Login("jack", "123456");
userLoginService.Login("rose", "123456");

// 基于对象创建代理对象
var userLoginService = (IUserLoginService)Larva.DynamicProxy.DynamicProxyFactory.CreateProxy(
    typeof(IUserLoginService),
    new UserLoginService(new UserLoginRepository()),
    new System.Type[] {
        typeof(UserLoginCounterInterceptor)
    });
userLoginService.Login("jack", "123456");
userLoginService.Login("rose", "123456");

// 基于类型创建代理类，代理类拥有和原始类相同的Public的构造函数
var userLoginServiceType = Larva.DynamicProxy.DynamicProxyFactory.CreateProxyType(
    typeof(IUserLoginService),
    typeof(UserLoginService),
    new System.Type[] {
        typeof(UserLoginCounterInterceptor)
    });
var userLoginService = (IUserLoginService)Activator.CreateInstance(
    userLoginServiceType,
    new object[] {
        new UserLoginRepository()
    });
userLoginService.Login("jack", "123456");
userLoginService.Login("rose", "123456");
```

## 更新历史

### 1.0.0 (更新日期：2019/10/01)

```plain
1）基于对象，返回指定接口的代理类对象，此代理类引用原始对象；
2）基于类型，返回值ID难过接口的代理类，此代理类拥有原始类Public的构造函数；
3）通过实现 IInvocation 接口，并将其实现类的类型作为参数传入创建代理或代理类，即可实现AOP。
```
