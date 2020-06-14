using System;
using System.Diagnostics;
using System.Threading;

namespace Larva.DynamicProxy.Tests.Interceptors
{
    public class PerformanceCounterInterceptor : Larva.DynamicProxy.StandardInterceptor
    {
        private AsyncLocal<Stopwatch> _sw = new AsyncLocal<Stopwatch>();
        private long _disposeCounter = 0;

        protected override void PreProceed(Larva.DynamicProxy.IInvocation invocation)
        {
            _sw.Value = new Stopwatch();
            _sw.Value.Start();
        }

        protected override void PostProceed(Larva.DynamicProxy.IInvocation invocation)
        {
            _sw.Value.Stop();
            var elapsedMilliseconds = _sw.Value.ElapsedMilliseconds;
            _sw.Value.Reset();
            Console.WriteLine($"{invocation.InvocationTarget.GetType().FullName}.{invocation.MemberName} {invocation.MemberOperateType} elapsed {elapsedMilliseconds}ms.");
        }

        public override void Dispose()
        {
            _sw.Value = null;
            System.Threading.Interlocked.Increment(ref _disposeCounter);
            Console.WriteLine($"{nameof(PerformanceCounterInterceptor)} disposed {_disposeCounter} times.");
        }
    }
}
