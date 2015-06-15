using Gum.Core.IL;
using Gum.Core.IL.Commands;
using Gum.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.VM.Test
{
    class Program
    {
        static void Test1(string[] args)
        {
            var WriteLineFunc = new ExternFunction("WriteLine", GlobalDomain.VoidType, new List<IType>{
                GlobalDomain.StringType,
                GlobalDomain.IntType,
                GlobalDomain.IntType }, 

                (IValue[] values) => {
                    var s0 = ((StringValue)((RefValue)values[0]).Value).Value;
                    var i1 = ((IntValue)values[1]).Value;
                    var i2 = ((IntValue)values[2]).Value;

                    Console.WriteLine(s0, i1, i2);
                    return null;
                }
            );

            // 프로그램을 만들어서 테스트
            var interpreter = new Interpreter(
                new IType[] { 
                    GlobalDomain.StringType,
                    GlobalDomain.IntType,
                    GlobalDomain.IntType,
                    GlobalDomain.FuncType(GlobalDomain.VoidType, GlobalDomain.StringType, GlobalDomain.IntType, GlobalDomain.IntType)
                    }
                );

            // Call 테스트
            interpreter.Visit(new Move(0, new RefValue(new StringValue("{0}, {1}"))));
            interpreter.Visit(new Move(1, new IntValue(4)));
            interpreter.Visit(new Move(2, new IntValue(5)));
            interpreter.Visit(new Move(3, new FuncValue(WriteLineFunc)));

            interpreter.Visit(new StaticCall(-1, 3, new List<int> { 0, 1, 2 }));
        }

        // CompositeObject
        static void Test2(string[] args)
        {
            // RefType이 두가지 의미로 쓰이고 있다
            // 1) RefType, ValueType
            // 2) RefValue의 Type
           
            // RefType은 New로 RefType(
            // ValueType은 

            var SomeClassType = new RefType("SomeClass", new IType[] { 
                GlobalDomain.IntType,
                GlobalDomain.StringType,
            });

            var WriteLineFunc = new ExternFunction("WriteLine", GlobalDomain.VoidType, new List<IType>{
                GlobalDomain.StringType, 
                GlobalDomain.IntType,
                GlobalDomain.IntType }, 
                (IValue[] values) =>
                {
                    var s0 = ((StringValue)((RefValue)values[0]).Value).Value;
                    var i1 = ((IntValue)values[1]).Value;
                    var i2 = ((IntValue)values[2]).Value;

                    Console.WriteLine(s0, i1, i2);
                    return null;
                });

            // 프로그램을 만들어서 테스트
            var interpreter = new Interpreter(
                new IType[] { 
                    GlobalDomain.TypeType, // 실제 타입들이 들어가야 합니다
                    SomeClassType,         // SomeClassType RefType이므로 
                    GlobalDomain.RefType,  // 
                    GlobalDomain.IntType,  // IntType
                    GlobalDomain.RefType,  // StringType
                    GlobalDomain.RefType, 
                    GlobalDomain.IntType,
                    GlobalDomain.RefType,
                    GlobalDomain.FuncType(GlobalDomain.VoidType, GlobalDomain.StringType, GlobalDomain.IntType, GlobalDomain.IntType)
                }); 

            interpreter.Visit(new Move(0, new RefValue(new TypeValue(SomeClassType))));

            // New
            interpreter.Visit(new New(1, 0, Enumerable.Empty<int>()));

            // FieldRef
            interpreter.Visit(new FieldRef(2, 1, 0));
            interpreter.Visit(new Move(3, new IntValue(10)));
            interpreter.Visit(new Store(2, 3));

            interpreter.Visit(new FieldRef(4, 1, 1));
            interpreter.Visit(new Move(5, new RefValue(new StringValue("aaa {0}, {1}"))));
            interpreter.Visit(new Store(4, 5));

            interpreter.Visit(new Load(6, 2));
            interpreter.Visit(new Load(7, 4));

            interpreter.Visit(new Move(8, new FuncValue(WriteLineFunc)));                    
            interpreter.Visit(new StaticCall(-1, 8, new List<int> { 7, 6, 6}));
        }

        static void Main(String[] args)
        {
            Test1(args);
            Test2(args);

        }
    }
}
