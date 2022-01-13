using Gum.Collections;
using System;
using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.Analysis
{    
    public class ClassMemberVarSymbol : ISymbolNode
    {
        SymbolFactory factory;
        ClassSymbol outer;
        ClassMemberVarDeclSymbol decl;        
        TypeEnv typeEnv;

        internal ClassMemberVarSymbol(SymbolFactory factory, ClassSymbol outer, ClassMemberVarDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;

            typeEnv = outer.GetTypeEnv();
        }

        public ClassMemberVarSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeClassMemberVar(appliedOuter, decl);
        }        

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public R.Path.Nested MakeRPath()
        {
            var rname = RItemFactory.MakeName(decl.GetName());
            return new R.Path.Nested(outer.MakeRPath(), rname, R.ParamHash.None, default);
        }

        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        // for return type covariance
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        R.Path.Normal ISymbolNode.MakeRPath() => MakeRPath();

        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            return default;
        }
    }
}
