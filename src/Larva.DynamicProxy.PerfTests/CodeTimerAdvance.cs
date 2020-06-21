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

        public static void TimeByConsole(string title, int times, Action<int> action)
        {
            ConsoleColor foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("{0,16}：\r\n", title);
            Console.ForegroundColor = ConsoleColor.Cyan;

            var codeTimerAdvance = new CodeTimerAdvance
            {
                Times = times,
                Action = action,
                ShowProgress = true
            };
            codeTimerAdvance.Execute();

            Console.WriteLine(codeTimerAdvance.ToString());
            Console.ForegroundColor = foregroundColor;
        }

        private void Execute()
        {
            if (Times <= 0)
            {
                throw new Exception("非法迭代次数！");
            }
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            var previousGCCounts = new List<int>();
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                previousGCCounts.Add(GC.CollectionCount(i));
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < Times; i++)
            {
                Action?.Invoke(i);
            }
            stopwatch.Stop();
            Elapsed = stopwatch.Elapsed;
            var currentGCCounts = new List<int>();
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                currentGCCounts.Add(GC.CollectionCount(i) - previousGCCounts[i]);
            }
            Gen = currentGCCounts.ToArray();
        }

        public override string ToString()
        {
            return string.Format("\tExcute Time:\t{0,7:n0}ms\r\n \tGC[Gen]:\t{1,6}/{2}/{3}\r\n", Elapsed.TotalMilliseconds, Gen[0], Gen[1], Gen[2]);
        }
    }
}
