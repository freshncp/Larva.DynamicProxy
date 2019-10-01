using DynamicProxyTests.Application;
using System;
using System.Collections.Concurrent;

namespace DynamicProxyTests.Interceptors
{
    public class UserLoginCounterInterceptor : Larva.DynamicProxy.StandardInterceptor
    {
        private static ConcurrentDictionary<string, long> _counter = new ConcurrentDictionary<string, long>();

        protected override void PreProceed(Larva.DynamicProxy.IInvocation invocation)
        {
            base.PreProceed(invocation);
        }

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
}
