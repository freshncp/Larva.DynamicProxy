using System;
using Larva.DynamicProxy.PerfTests.Interceptors;

namespace Larva.DynamicProxy.PerfTests
{
    public class PerformanceAop
    {
        private const int COUNT = 1000000;

        public static void RunTest()
        {
            RunLarvaDynamicProxy();
            RunCastleDynamicProxy();
        }

        private static void RunLarvaDynamicProxy()
        {
            var proxy = Larva.DynamicProxy.DynamicProxyFactory.CreateProxy<IProxyImplementInterface>(new TestProxyImplementInterface(), new ValidateInterceptor(), new LogInterceptor());
            var a = 1;
            var b = "2";
            var b2 = new string[] { "2" };
            var c = new ValueA { Value = 3 };
            var d = 0L;
            CodeTimerAdvance.TimeByConsole("Larva.DynamicProxy.TestMethodWithRefAndOutParameter", COUNT, times =>
            {
                var result = proxy.TestMethodWithRefAndOutParameter(ref a, ref b2, out d);
                if(result != d)
                {
                    throw new ApplicationException("TestMethodWithRefAndOutParameter fail");
                }

            });
            CodeTimerAdvance.TimeByConsole("Larva.DynamicProxy.TestNormalMethod", COUNT, times =>
            {
                var result = proxy.TestNormalMethod(a, b);
                if (result != 3)
                {
                    throw new ApplicationException("TestNormalMethod fail");
                }
            });
            CodeTimerAdvance.TimeByConsole("Larva.DynamicProxy.TestMethodWithGenericParameter", COUNT, times =>
            {
                var result = proxy.TestMethodWithGenericParameter(a, b, new ValueA[] { c });
                if (result != 3 + c.Value)
                {
                    throw new ApplicationException("TestMethodWithGenericParameter fail");
                }
            });
            CodeTimerAdvance.TimeByConsole("Larva.DynamicProxy.TestMethodWithGenericParameterAndRefParameter", COUNT, times =>
            {
                var result = proxy.TestMethodWithGenericParameterAndRefParameter(a, b, ref c);
                if (result != c.Value)
                {
                    throw new ApplicationException("TestMethodWithGenericParameterAndRefParameter fail");
                }
            });
        }

        private static void RunCastleDynamicProxy()
        {
            var proxy = new Castle.DynamicProxy.ProxyGenerator().CreateInterfaceProxyWithTarget<IProxyImplementInterface>(new TestProxyImplementInterface(), new ValidateInterceptor(), new LogInterceptor());
            var a = 1;
            var b = "2";
            var b2 = new string[] { "2" };
            var c = new ValueA { Value = 3 };
            var d = 0L;
            CodeTimerAdvance.TimeByConsole("Castle.DynamicProxy.TestMethodWithRefAndOutParameter", COUNT, times =>
            {
                var result = proxy.TestMethodWithRefAndOutParameter(ref a, ref b2, out d);
                if (result != d)
                {
                    throw new ApplicationException("TestMethodWithRefAndOutParameter fail");
                }
            });
            CodeTimerAdvance.TimeByConsole("Castle.DynamicProxy.TestNormalMethod", COUNT, times =>
            {
                var result = proxy.TestNormalMethod(a, b);
                if (result != 3)
                {
                    throw new ApplicationException("TestNormalMethod fail");
                }
            });
            CodeTimerAdvance.TimeByConsole("Castle.DynamicProxy.TestMethodWithGenericParameter", COUNT, times =>
            {
                var result = proxy.TestMethodWithGenericParameter(a, b, new ValueA[] { c });
                if (result != 3 + c.Value)
                {
                    throw new ApplicationException("TestMethodWithGenericParameter fail");
                }
            });
            CodeTimerAdvance.TimeByConsole("Castle.DynamicProxy.TestMethodWithGenericParameterAndRefParameter", COUNT, times =>
            {
                var result = proxy.TestMethodWithGenericParameterAndRefParameter(a, b, ref c);
                if (result != c.Value)
                {
                    throw new ApplicationException("TestMethodWithGenericParameterAndRefParameter fail");
                }
            });
        }
    }

    #region 测试数据
    public interface IProxyImplementInterface
    {
        int TestMethodWithRefAndOutParameter(ref int a, ref string[] b, out long d);

        int TestNormalMethod(int a, string b);

        int TestMethodWithGenericParameter<T>(int a, string b, T[] c)
            where T : IValue;

        int TestMethodWithGenericParameterAndRefParameter<T>(int a, string b, ref T c)
            where T : IValue;
    }

    public interface IValue
    {
        int Value { get; set; }
    }

    public class ValueA : IValue
    {
        public int Value { get; set; }
    }

    public class TestProxyImplementInterface : IProxyImplementInterface
    {
        public virtual int TestMethodWithRefAndOutParameter(ref int a, ref string[] b, out long d)
        {
            var result = a + Convert.ToInt32(b[0]);
            d = result;
            return result;
        }

        public virtual int TestNormalMethod(int a, string b)
        {
            return a + Convert.ToInt32(b);
        }

        public virtual int TestMethodWithGenericParameter<T>(int a, string b, T[] c)
            where T : IValue
        {
            return a + Convert.ToInt32(b) + c[0].Value;
        }

        public virtual int TestMethodWithGenericParameterAndRefParameter<T>(int a, string b, ref T c)
            where T : IValue
        {
            var result = a + Convert.ToInt32(b);
            c.Value = result;
            return result;
        }
    }
    #endregion
}
