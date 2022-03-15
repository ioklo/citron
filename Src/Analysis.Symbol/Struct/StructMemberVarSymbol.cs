using System;
using Citron.Collections;
using Citron.Infra;
using M = Citron.CompileTime;

namespace Citron.Analysis
{
    public class StructMemberVarSymbol : ISymbolNode
    {
        SymbolFactory factory;
        StructSymbol outer;
        StructMemberVarDeclSymbol decl;

        internal StructMemberVarSymbol(SymbolFactory factory, StructSymbol outer, StructMemberVarDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
        }

        public StructMemberVarSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeStructMemberVar(appliedOuter, decl);
        }

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public M.Name GetName()
        {
            return decl.GetName();
        }

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public ITypeSymbol GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
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