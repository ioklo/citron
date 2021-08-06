using Gum.Test.IntegrateTest;
using System;
using System.IO;

namespace TestCodeExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = IntegrateTestData.GetInfosCount();

            for(int i = 0; i < count; i++)
            {
                var info = IntegrateTestData.GetInfo(i);

                var testData = info.MakeTestData();
                var code = testData.GetCode();
                if (code == null) continue;

                var tempDir = Directory.CreateDirectory("temp");

                using (var writer = new StreamWriter(Path.Combine("temp", $"{info.Desc}.qs")))
                    writer.Write(code);
            }
        }
    }
}
