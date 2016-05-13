using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Gum.Test.Text2AST
{
    abstract class TestBase<TTestCase> : ITest where TTestCase : ITestCase
    {
        abstract public void ConfigDeserializer(Deserializer deserializer);
        abstract public bool Test(TTestCase testCase);

        public void Test(byte[] testResource)
        {
            var memoryStream = new MemoryStream(testResource);
            var reader = new StreamReader(memoryStream, Encoding.UTF8);

            var deserializer = new Deserializer();
            ConfigDeserializer(deserializer);

            var testCases = deserializer.Deserialize<List<TTestCase>>(reader);

            int count = 0;
            foreach (var testCase in testCases)
            {
                count++;
                Console.WriteLine("Test #{0} '{1}':", count.ToString(), testCase.TestName);

                bool bSuccess = Test(testCase);

                if (bSuccess)
                    Console.WriteLine("    Succeed");
                else
                {
                    Console.WriteLine("    Fail");
                    return;
                }
            }
        }
    }
}
