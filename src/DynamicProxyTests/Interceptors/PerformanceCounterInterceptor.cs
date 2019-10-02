﻿using System;
using System.Diagnostics;

namespace DynamicProxyTests.Interceptors
{
    public class PerformanceCounterInterceptor : Larva.DynamicProxy.StandardInterceptor
    {
        private Stopwatch _sw = new Stopwatch();
        protected override void PreProceed(Larva.DynamicProxy.IInvocation invocation)
        {
            _sw.Start();
        }

        protected override void PostProceed(Larva.DynamicProxy.IInvocation invocation)
        {
            _sw.Stop();
            var elapsedMilliseconds = _sw.ElapsedMilliseconds;
            Console.WriteLine($"{invocation.MethodInvocationTarget.DeclaringType.FullName}.{invocation.MethodInvocationTarget.Name} elapsed {elapsedMilliseconds}ms.");
        }

        public override void Dispose()
        {
            Console.WriteLine($"{nameof(PerformanceCounterInterceptor)} disposed");
        }
    }
}