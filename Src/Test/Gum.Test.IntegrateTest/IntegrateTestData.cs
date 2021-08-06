using Gum.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gum.Test.IntegrateTest
{
    public class IntegrateTestData
    {
        static bool bInit = false;
        
        // DONT TRUST static constructor
        public static void Initialize()
        {
            if (bInit) return;
            bInit = true;

            ClassTestData.Initialize();
            StructTestData.Initialize();            
        }

        static Dictionary<Type, List<(string Desc, Func<TestData> MakeTestData)>> infos;

        public static (string Desc, Func<TestData> MakeTestData) GetInfo(Type type, int index)
        {
            return infos[type][index];
        }

        public static void AddInfo(Type type, string desc, Func<TestData> makeTestData)
        {
            if (!infos.TryGetValue(type, out var list))
            {
                list = new List<(string Desc, Func<TestData> MakeTestData)>();
                infos.Add(type, list);
            }

            list.Add((desc, makeTestData));
        }

        public static int GetInfosCount()
        {
            return infos.Values.Sum(value => value.Count);
        }

        public static IEnumerable<(string typeName, string Desc, Func<TestData> MakeTestData)> GetAllInfos()
        {

            foreach (var (type, list) in infos)
            {
                foreach (var item in list)
                    yield return (type.Name, item.Desc, item.MakeTestData);
            }
        }

        static IntegrateTestData()
        {
            infos = new Dictionary<Type, List<(string Desc, Func<TestData> MakeTestData)>>();
            Initialize();
        }
    }

    public class IntegrateTestData<TTestData> : IEnumerable<object[]>
    {
        static int count;
        static bool bInit;
        
        static IntegrateTestData()
        {
            bInit = false;
            IntegrateTestData.Initialize();
        }

        public static void Initialize()
        {
            if (bInit) return;
            bInit = true;

            var regex = new Regex(@"Make_([\w_]+)$");
            
            count = 0;
            foreach (var methodInfo in typeof(TTestData).GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
            {
                var match = regex.Match(methodInfo.Name);
                if (!match.Success) continue;

                var desc = match.Groups[1].Value;
                Func<TestData> invoker = () => ((TestData)methodInfo.Invoke(null, null)!);
                IntegrateTestData.AddInfo(typeof(TTestData), $"F{(count + 1).ToString("D2")}_{desc}", invoker);

                count++;
            }
        }

        public IEnumerable<TestDataInfo> GetInfos()
        {
            for (int i = 0; i < count; i++)
                yield return new TestDataInfo(typeof(TTestData), i);
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            Debugger.Break();

            foreach (var info in GetInfos())
                yield return new object[] { info };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Debugger.Break();

            return GetEnumerator();
        }
    }
}
