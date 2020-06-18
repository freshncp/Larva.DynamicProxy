using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Larva.DynamicProxy.PerfTests
{
    public class CodeTimerAdvance
    {
        public int Times
        {
            get;
            set;
        }

        public Action<int> Action
        {
            get;
            set;
        }

        public bool ShowProgress
        {
            get;
            set;
        }

        public int Index
        {
            get;
            set;
        }

        public int[] Gen
        {
            get;
            set;
        }

        public TimeSpan Elapsed
        {
            get;
            set;
        }

        public CodeTimerAdvance()
        {
            int[] array2 = Gen = new int[3];
        }

        public static CodeTimerAdvance Time(int times, Action<int> action)
        {
            var codeTimerAdvance = new CodeTimerAdvance
            {
                Times = times,
                Action = action,
                ShowProgress = true
            };
            codeTimerAdvance.TimeOne();
            codeTimerAdvance.Time();
            return codeTimerAdvance;
        }

        public static void TimeByConsole(string title, int times, Action<int> action)
        {
            ConsoleColor foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("{0,16}：\r\n", title);
            Console.ForegroundColor = ConsoleColor.Cyan;
            var codeTimerAdvance2 = Time(times, action);
            Console.WriteLine(codeTimerAdvance2.ToString());
            Console.ForegroundColor = foregroundColor;
        }

        public virtual void Time()
        {
            if (Times <= 0)
            {
                throw new Exception("非法迭代次数！");
            }
            RealExcute();
        }

        protected virtual void RealExcute()
        {
            if (Times <= 0)
            {
                throw new Exception("非法迭代次数！");
            }
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            int[] array = new int[GC.MaxGeneration + 1];
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                array[i] = GC.CollectionCount(i);
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Action<int> action = Action;
            if (action == null)
            {
                action = Time;
                BeforeExcute();
            }
            for (int i = 0; i < Times; i++)
            {
                Index = i;
                action(i);
            }
            if (Action == null)
            {
                EndExcute();
            }
            stopwatch.Stop();
            Elapsed = stopwatch.Elapsed;
            List<int> list = new List<int>();
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                int item = GC.CollectionCount(i) - array[i];
                list.Add(item);
            }
            Gen = list.ToArray();
        }

        public void TimeOne()
        {
            int times = Times;
            try
            {
                Times = 1;
                Time();
            }
            finally
            {
                Times = times;
            }
        }

        public virtual void BeforeExcute()
        {
        }

        public virtual void Time(int index)
        {
        }

        public virtual void EndExcute()
        {
        }

        public override string ToString()
        {
            return string.Format("\tExcute Time:\t{0,7:n0}ms\r\n \tGC[Gen]:\t{1,6}/{2}/{3}\r\n", Elapsed.TotalMilliseconds, Gen[0], Gen[1], Gen[2]);
        }
    }
}
