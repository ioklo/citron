using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gum.App.Compiler.AST;

namespace Gum.App.Compiler
{
    internal class TypeCheckerContext
    {
        // manage whole types
        public TypeManager TypeManager { get; private set; }

        public List<Tuple<string, TypeInfo>> VarTypes { get; private set; }
        public FuncDecl CurFunc { get; set; }
        public List<FuncDecl> FuncDecls { get; set; }

        Stack<int> scopes = new Stack<int>();

        public TypeCheckerContext(TypeManager tm)
        {
            FuncDecls = new List<FuncDecl>();
            VarTypes = new List<Tuple<string, TypeInfo>>();
            TypeManager = tm;
        }

        public void AddVarType(string var, TypeInfo type)
        {
            VarTypes.Add(Tuple.Create(var, type));
        }

        public bool TryGetVarType(string var, out TypeInfo type)
        {
            int index = VarTypes.FindLastIndex(tuple => tuple.Item1 == var);
            if (index == -1)
            {
                type = null;
                return false;
            }

            type = VarTypes[index].Item2;
            return true;
        }

        public FuncDecl GetFunc(string p)
        {
            return FuncDecls.Find(decl => decl.Name == p);
        }

        internal void PushScope()
        {
            scopes.Push(VarTypes.Count);
        }

        internal void PopScope()
        {
            int count = scopes.Pop();

            VarTypes.RemoveRange(count, VarTypes.Count - count);
        }        
    }
}
