using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Gum.Core.IL;
using Gum.App.Compiler;
using System.IO;

namespace Gum.Test.VM
{
    using Parser = Gum.App.Compiler.Parser;
    using Program = Gum.Core.IL.Program;
    using System.Collections.Generic;
    using Gum.Core.IL.Commands;

    [TestClass]
    public class BasicTest
    {
        public Program Compile(string fileName)
        {
            string code;
            using (var reader = new StreamReader(fileName))
                code = reader.ReadToEnd();
            
            Domain domain = new Domain();

            return Compiler.Compile(domain, code);
        }

        [TestMethod]
        public void TestMethod1()
        {
            const int input = 3;

            List<ICommand> cmds = new List<ICommand>();

            cmds.Add(new Push(input));
            cmds.Add(new Return());

            Domain domain = new Domain();
            domain.AddVariable("main", new Function("main", 0, 0, 1, cmds, new List<int>()));

            Program prog = new Program(domain);           
            
            Gum.App.VM.Interpreter interp = new Gum.App.VM.Interpreter();
            int ret = (int)interp.Call(domain, "main");

            Assert.AreEqual(input, ret);
        }

        public object WriteLine(object[] ps)
        {
            Console.WriteLine(ps[0]);
            return null;
        }

        [TestMethod]
        public void TestFiles()
        {
            // 3단계 올라가서 
            var files = new [] { 
                // @"..\..\GumTest\Tests\if.gum",
                // @"..\..\GumTest\Tests\for.gum",
                @"..\..\GumTest\Tests\shortcircuit.gum",
                // @"..\..\GumTest\Tests\class\field.gum",



            };

            foreach (var file in files )
            {
                Program prog = Compile(file);

                Domain domain = new Domain();
                ExternFunction externFunc = new Gum.Core.IL.ExternFunction("WriteLine", 1, 1);
                domain.AddVariable("WriteLine", externFunc);
                 
                Gum.App.VM.Interpreter interpreter = new Gum.App.VM.Interpreter();
                interpreter.AddExternFunc("WriteLine", WriteLine);
                var obj = interpreter.Call(domain, "test");

                Assert.IsTrue((bool)obj);
            }
        }
    }
}
