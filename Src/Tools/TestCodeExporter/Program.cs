using Gum.Test.IntegrateTest;
using System;
using System.IO;

namespace TestCodeExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            IntegrateTestData.Initialize();            
            
            foreach (var info in IntegrateTestData.GetAllInfos())
            {
                // var info = IntegrateTestData.GetInfo(i);

                var testData = info.MakeTestData();
                var code = testData.GetCode();
                if (code == null) continue;

                // var tempDir = Directory.CreateDirectory("temp");
                var tempDir = Directory.CreateDirectory(Path.Combine("temp", info.typeName));

                using (var writer = new StreamWriter(Path.Combine("temp", info.typeName, $"{info.Desc}.qs")))
                    writer.Write(code);
            }            
        }
    }
}
