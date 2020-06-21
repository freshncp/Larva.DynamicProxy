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
            CodeTimerAdvance.TimeByConsole("Larva.DynamicProxy.TestMethodWithRefAndOutParameter", COUNT, times =>
            {
                var a = 1;
                var b = new string[] { "2" };
                var c = 0L;
                var result = proxy.TestMethodWithRefAndOutParameter(ref a, ref b, out c);
                if (result != c)
                {
                    throw new ApplicationException("Larva.DynamicProxy.TestMethodWithRefAndOutParameter fail");
                }
            });
            CodeTimerAdvance.TimeByConsole("Larva.DynamicProxy.TestNormalMethod", COUNT, times =>
            {
                var a = 1;
                var b = "2";
                var result = proxy.TestNormalMethod(a, b);
                if (result != (a + 1) + Convert.ToInt32(b))
                {
                    throw new ApplicationException($"Larva.DynamicProxy.TestNormalMethod fail, actual:{result}, expected:{(a + 1) + Convert.ToInt32(b)}");
                }
            });
            CodeTimerAdvance.TimeByConsole("Larva.DynamicProxy.TestMethodWithGenericParameter", COUNT, times =>
            {
                var a = 1;
                var b = "2";
                var c = new ValueA { Value = 3 };
                var result = proxy.TestMethodWithGenericParameter(a, b, new ValueA[] { c });
                if (result != (a + 1) + Convert.ToInt32(b) + c.Value)
                {
                    throw new ApplicationException($"Larva.DynamicProxy.TestMethodWithGenericParameter fail, actual:{result}, expected:{(a + 1) + Convert.ToInt32(b) + c.Value}");
                }
            });
            CodeTimerAdvance.TimeByConsole("Larva.DynamicProxy.TestMethodWithGenericParameterAndRefParameter", COUNT, times =>
            {
                var a = 1;
                var b = "2";
                var c = new ValueA { Value = 3 };
                var result = proxy.TestMethodWithGenericParameterAndRefParameter(a, b, ref c);
                if (result != c.Value)
                {
                    throw new ApplicationException("Larva.DynamicProxy.TestMethodWithGenericParameterAndRefParameter fail");
                }
            });
        }

        private static void RunCastleDynamicProxy()
        {
            var proxy = new Castle.DynamicProxy.ProxyGenerator().CreateInterfaceProxyWithTarget<IProxyImplementInterface>(new TestProxyImplementInterface(), new ValidateInterceptor(), new LogInterceptor());
            CodeTimerAdvance.TimeByConsole("Castle.DynamicProxy.TestMethodWithRefAndOutParameter", COUNT, times =>
            {
                var a = 1;
                var b = new string[] { "2" };
                var c = 0L;
                var result = proxy.TestMethodWithRefAndOutParameter(ref a, ref b, out c);
                if (result != c)
                {
                    throw new ApplicationException("Castle.DynamicProxy.TestMethodWithRefAndOutParameter fail");
                }
            });
            CodeTimerAdvance.TimeByConsole("Castle.DynamicProxy.TestNormalMethod", COUNT, times =>
            {
                var a = 1;
                var b = "2";
                var result = proxy.TestNormalMethod(a, b);
                if (result != (a + 1) + Convert.ToInt32(b))
                {
                    throw new ApplicationException($"Castle.DynamicProxy.TestNormalMethod fail, actual:{result}, expected:{(a + 1) + Convert.ToInt32(b)}");
                }
            });
            CodeTimerAdvance.TimeByConsole("Castle.DynamicProxy.TestMethodWithGenericParameter", COUNT, times =>
            {
                var a = 1;
                var b = "2";
                var c = new ValueA { Value = 3 };
                var result = proxy.TestMethodWithGenericParameter(a, b, new ValueA[] { c });
                if (result != (a + 1) + Convert.ToInt32(b) + c.Value)
                {
                    throw new ApplicationException($"Castle.DynamicProxy.TestMethodWithGenericParameter fail, actual:{result}, expected:{(a + 1) + Convert.ToInt32(b) + c.Value}");
                }
            });
            CodeTimerAdvance.TimeByConsole("Castle.DynamicProxy.TestMethodWithGenericParameterAndRefParameter", COUNT, times =>
            {
                var a = 1;
                var b = "2";
                var c = new ValueA { Value = 3 };
                var result = proxy.TestMethodWithGenericParameterAndRefParameter(a, b, ref c);
                if (result != c.Value)
                {
                    throw new ApplicationException("Castle.DynamicProxy.TestMethodWithGenericParameterAndRefParameter fail");
                }
            });
        }
    }

    #region 测试数据
    public interface IProxyImplementInterface
    {
        int TestMethodWithRefAndOutParameter(ref int a, ref string[] b, out long c);

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
        public virtual int TestMethodWithRefAndOutParameter(ref int a, ref string[] b, out long c)
        {
            var result = a + Convert.ToInt32(b[0]);
            c = result;
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
