using Gum.Prerequisite.Generator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite
{
    class GenerateCoreIL
    {
        static Structure GenerateStucture()
        {
            var structure = new Structure();

            var stringType = structure.CreatePrimitive("string");
            var intType = structure.CreatePrimitive("int");
            var boolType = structure.CreatePrimitive("bool");
            // var valueType = structure.CreatePrimitive("IValue");
            
            var globalRefCmd = structure.CreateStruct("GlobalRefCmd");
            var localRefCmd = structure.CreateStruct("LocalRefCmd");
            var fieldRefCmd = structure.CreateStruct("FieldRefCmd");
            var newCmd = structure.CreateStruct("NewCmd");        
            var loadCmd = structure.CreateStruct("LoadCmd");
            var storeCmd = structure.CreateStruct("StoreCmd");
            var moveCmd = structure.CreateStruct("MoveCmd");
            var moveRegCmd = structure.CreateStruct("MoveRegCmd");
            var jumpCmd = structure.CreateStruct("JumpCmd");
            var ifNotJumpCmd = structure.CreateStruct("IfNotJumpCmd");
            var staticCallCmd = structure.CreateStruct("StaticCallCmd");
            var virtualCallCmd = structure.CreateStruct("VirtualCallCmd");
            var returnCmd = structure.CreateStruct("ReturnCmd");

            var cmd = structure.CreateComponent("ICommand");

            cmd
                .Add(globalRefCmd)
                .Add(localRefCmd)
                .Add(fieldRefCmd)
                .Add(newCmd)
                .Add(loadCmd)
                .Add(storeCmd)
                .Add(moveCmd)
                .Add(moveRegCmd)
                .Add(jumpCmd)
                .Add(ifNotJumpCmd)
                .Add(staticCallCmd)
                .Add(virtualCallCmd)
                .Add(returnCmd);

            globalRefCmd
                .Var(intType, "DestReg")
                .Var(intType, "Index");

            localRefCmd
                .Var(intType, "DestReg")
                .Var(intType, "Index");

            fieldRefCmd
                .Var(intType, "DestReg")
                .Var(intType, "SrcRefReg")
                .Var(intType, "Index");

            newCmd
                .Var(intType, "DestReg")
                .Var(intType, "Type")
                .Vars(intType, "TypeArgs");

            loadCmd
                .Var(intType, "Dest")
                .Var(intType, "SrcRef");

            storeCmd
                .Var(intType, "DestRef")
                .Var(intType, "Src");

            moveCmd
                .Var(intType, "Dest")
                .Var(intType, "ValueIndex");

            moveRegCmd
                .Var(intType, "DestReg")
                .Var(intType, "SrcReg");

            jumpCmd
                .Var(intType, "Block");

            ifNotJumpCmd
                .Var(intType, "Cond")
                .Var(intType, "Block");

            staticCallCmd
                .Var(intType, "Ret")
                .Var(intType, "Func")
                .Vars(intType, "Args");

            virtualCallCmd
                .Var(intType, "Ret")
                .Var(intType, "Func")
                .Vars(intType, "Args");

            returnCmd
                .Var(intType, "Value");

            return structure;
        }

        static public void GenerateCSharpFile()
        {
            var structure = GenerateStucture();

            // Src
            using(var streamWriter = new StreamWriter(@"..\..\Src\Gum.Lang\CoreIL\Generated.cs"))
            {
                var printer = new CSharpPrinter("Gum.Lang.CoreIL");
                
                structure.Print(printer, streamWriter);
            }
        }
    }
}
