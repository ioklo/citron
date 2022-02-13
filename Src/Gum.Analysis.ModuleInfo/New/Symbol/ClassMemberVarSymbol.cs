using Gum.Collections;
using System;
using M = Gum.CompileTime;

namespace Citron.Analysis
{    
    public class ClassMemberVarSymbol : ISymbolNode
    {
        SymbolFactory factory;
        ClassSymbol outer;
        ClassMemberVarDeclSymbol decl;        
        TypeEnv typeEnv;

        // for return type covariance
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode? ISymbolNode.GetOuter() => GetOuter();

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

        public ClassSymbol GetOuter()
        {
            return outer;
        }
        
        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return default;
        }

        public ITypeSymbol GetDeclType()
        {
            var declType = decl.GetDeclType();
            var typeEnv = GetTypeEnv();

            return declType.Apply(typeEnv);
        }

        public bool IsStatic()
        {
            return decl.IsStatic();
        }
    }
}
