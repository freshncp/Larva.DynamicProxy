using Larva.DynamicProxy.Tests.Application;
using System;
using System.Collections.Concurrent;

namespace Larva.DynamicProxy.Tests.Interceptors
{
    public class UserLoginCounterInterceptor : Larva.DynamicProxy.StandardInterceptor
    {
        private ConcurrentDictionary<string, long> _counter = new ConcurrentDictionary<string, long>();
        private long _disposeCounter = 0;

        protected override void PreProceed(Larva.DynamicProxy.IInvocation invocation)
        {
            Console.WriteLine("Begin login");
        }

        protected override void PostProceed(Larva.DynamicProxy.IInvocation invocation)
        {
            if (invocation.InvocationTarget is IUserLoginService
                && invocation.MemberType == System.Reflection.MemberTypes.Method
                && invocation.MemberOperateType == Larva.DynamicProxy.MemberOperateTypes.None
                && (invocation.MemberName == nameof(IUserLoginService.Login)
                    || invocation.MemberName == nameof(IUserLoginService.LoginAsync)))
            {
                var userName = (string)invocation.Arguments[0];
                _counter.TryAdd(userName, 0);
                _counter.AddOrUpdate(userName, 0, (key, originVal) => System.Threading.Interlocked.Increment(ref originVal));
                Console.WriteLine($"{userName} has login {_counter[userName]} times");
                Console.WriteLine($"Welcome {userName}!");
            }
        }

        public override void Dispose()
        {
            System.Threading.Interlocked.Increment(ref _disposeCounter);
            Console.WriteLine($"{nameof(UserLoginCounterInterceptor)} disposed {_disposeCounter} times.");
        }
    }
}
