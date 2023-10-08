using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Citron.Test.IntegrateTest
{
    public class Tests
    {
        public Tests()
        {
            IntegrateTestData.Initialize();
        }

        [Theory]
        [ClassData(typeof(StructTestData))]
        public Task T01_StructTest(TestDataInfo testDataInfo)
        {
            return testDataInfo.InvokeAsync();
        }

        [Theory]
        [ClassData(typeof(ClassTestData))]
        public Task T02_ClassTest(TestDataInfo testDataInfo)
        {
            return testDataInfo.InvokeAsync();
        }
    }
}
