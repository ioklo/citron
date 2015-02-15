using System;
using System.Collections.Generic;
using Gum.Core.IL;
using Gum.App.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gum.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            List<ICommand> cmds = new List<ICommand>()
            {
                new Push(3),
                new Push(4),
                new Operator(OperatorKind.Neg),
                new Operator(OperatorKind.Sub),
                new Return() 
            };

            Program prog = new Program();
            prog.AddFunc("myF", 0, 0, 1, cmds, new List<int>());
  
            Interpreter interp = new Interpreter(prog);
            int iv = (int)interp.Call("myF");
            Assert.AreEqual(iv, 7);
        }

        /*[TestMethod]
        public void FieldTest1()
        {
            // var obj = {};
            // obj.ku = "MyString";
            // return obj.ku;

            var loc_2_ku = new Location();
            loc_2_ku.Reg = 2;
            loc_2_ku.Fields.Add(new StringFieldIndicator() { Name = "ku" });

            List<ICommand> cmds = new List<ICommand>()
            {
                new NewObject() 
                {
                    Reg = 7
                },

                new NewObject() 
                {
                    Reg = 2
                },

                new Put()            
                {
                    Loc = loc_2_ku,
                    Exp = new StringValue() { Value = "MyString" }
                },

                new Return() 
                {
                    Result = new Get { Loc = loc_2_ku }
                }
            };

            BasicBlock bb = new BasicBlock();
            bb.Commands.AddRange(cmds);

            Function func = new Function("myF");
            Program prog = new Program();
            prog.AddFunc(func, bb);

            Interpreter interp = new Interpreter(prog);

            StringValue iv = (StringValue)interp.Call(func);
            Assert.AreEqual(iv.Value, "MyString");
        }*/
    }
}
