using Gum.Collections;
using System;
using M = Gum.CompileTime;

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
        
        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        // for return type covariance
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);        

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
    }
}
