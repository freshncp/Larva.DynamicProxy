# Larva.DynamicProxy

dotnet 动态代理类，用于AOP。可以结合IoC框架。此动态代理仅支持通过实现接口来创建代理类。

- 基于对象，返回指定接口的代理类对象，此代理类引用原始对象；

- 基于类型，返回指定接口的代理类，此代理类拥有原始类Public的构造函数；

- 通过实现IInterceptor接口或继承StandardInterceptor类，并将其对象实例作为参数传入创建代理或代理类，即可实现拦截方法（Method）、属性（Property）；

- StandardInterceptor 支持拦截Task异步方法。

## 安装

```sh
dotnet add package Larva.DynamicProxy
```

## 使用

示例参见：[Larva.DynamicProxy.Tests](src/Larva.DynamicProxy.Tests)

## 发布历史

### 2.0.0-beta2 (更新日期：2020/06/18)

```plain
1）优化动态代理IL生成；
2）修复StandardInterceptor，拦截异步方法时，Dispose的调用应仍在主线程里执行，确保类似AsyncLocal变量在主线程上被释放；
3）优化StandardInterceptor，对PostProceed、ExceptionThrown、Dispose的调用，捕获异常抛出；
4）小重构：调整拦截器名字空间。
```

### 2.0.0-beta1 (更新日期：2020/06/14)

```plain
1）重构，取消反射调用，改为委托调用；
2）拦截器，由类型改为对象传入。
```

### 1.0.7 (更新日期：2019/12/17)

```plain
1）修复方法包含out/ref参数时，报错的问题
```

### 1.0.6 (更新日期：2019/10/13)

```plain
1）支持dotNetFramework4.5及以上版本
```

### 1.0.5 (更新日期：2019/10/03)

```plain
1）StandardInterceptor 修复在异步环境下Dispose方法多一次提前执行的bug；
2）StandardInterceptor 修复在异步环境下PreProceed出错时会继续执行PostProceed的bug。
```

### 1.0.4 (更新日期：2019/10/02)

```plain
1）StandardInterceptor 修复在异步方法上执行出错时，仍继续执行PostProceed的bug。
```

### 1.0.3 (更新日期：2019/10/02)

```plain
1）StandardInterceptor 支持拦截Task异步方法。
```

### 1.0.2 (更新日期：2019/10/02)

```plain
1）增加支持Standard1.6。
```

### 1.0.1 (更新日期：2019/10/02)

```plain
1）重新打nuget包。
```

### 1.0.0 (更新日期：2019/10/01)

```plain
1）基于对象，返回指定接口的代理类对象，此代理类引用原始对象；
2）基于类型，返回指定接口的代理类，此代理类拥有原始类Public的构造函数；
3）通过实现IInterceptor 接口或继承StandardInterceptor类，并将其实现类的类型作为参数传入创建代理或代理类，即可实现拦截方法（Method）、属性（Property）。
```
