using Gum.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gum.Test.IntegrateTest
{
    public class IntegrateTestData
    {
        static List<(string Desc, Func<TestData> MakeTestData)> infos;
        public static object lockObj;        

        public static (string Desc, Func<TestData> MakeTestData) GetInfo(int index) => infos[index];
        protected static void AddInfo(string desc, Func<TestData> makeTestData)
        {
            infos.Add((desc, makeTestData));
        }
        public static int GetInfosCount() => infos.Count;

        static IntegrateTestData()
        {
            infos = new List<(string Desc, Func<TestData> MakeTestData)>();
            lockObj = new object();
        }
    }

    class IntegrateTestData<TTestData> : IntegrateTestData, IEnumerable<object[]>
    {
        static int startIndex, endIndex;

        static IntegrateTestData()
        {
            var regex = new Regex(@"Make_([\w_]+)$");

            lock (lockObj)
            {
                startIndex = GetInfosCount();

                int count = 1;
                foreach (var methodInfo in typeof(TTestData).GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
                {
                    var match = regex.Match(methodInfo.Name);
                    if (!match.Success) continue;

                    var desc = match.Groups[1].Value;
                    Func<TestData> invoker = () => ((TestData)methodInfo.Invoke(null, null)!);
                    AddInfo($"F{count.ToString("D2")}_{desc}", invoker);

                    count++;
                }

                endIndex = GetInfosCount();
            }
        }

        public IEnumerable<TestDataInfo> GetInfos()
        {
            for (int i = startIndex; i < endIndex; i++)
                yield return new TestDataInfo(i);
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (var info in GetInfos())
                yield return new object[] { info };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
