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

    [TestClass]
    public class BasicTest
    {
        public Program Compile(string fileName)
        {
            string code;
            using (var reader = new StreamReader(fileName))
                code = reader.ReadToEnd();
            
            Compiler compiler = new Compiler();
            return compiler.Compile(code);
        }

        [TestMethod]
        public void TestMethod1()
        {
            const int input = 3;

            List<ICommand> cmds = new List<ICommand>();

            cmds.Add(new Push(input));
            cmds.Add(new Return());

            Program prog = new Program();
            prog.AddFunc("main", 0, 0, 1, cmds, new List<int>());
            
            Gum.App.VM.Interpreter interp = new Gum.App.VM.Interpreter(prog);
            int ret = (int)interp.Call("main");

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

                Gum.App.VM.Interpreter interpreter = new Gum.App.VM.Interpreter(prog);
                interpreter.AddExternFunc("WriteLine", WriteLine);
                var obj = interpreter.Call("test");

                Assert.IsTrue((bool)obj);
            }
        }
    }
}
