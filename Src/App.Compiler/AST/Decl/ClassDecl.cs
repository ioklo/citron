using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gum.App.Compiler.AST;
using Gum.App.Compiler;

namespace Gum.App.Compiler.AST
{
    public class ClassDecl : IDecl
    {
        public string Name { get; private set; }
        public List<ClassFuncDecl> FuncDecls { get; private set; }
        public List<ClassVarDecl> VarDecls { get; private set; }
        public List<string> BaseTypes { get; private set; }

        public ClassDecl(string name)
        {
            Name = name;
            FuncDecls = new List<ClassFuncDecl>();
            VarDecls = new List<ClassVarDecl>();
            BaseTypes = new List<string>();
        }
    }
}
