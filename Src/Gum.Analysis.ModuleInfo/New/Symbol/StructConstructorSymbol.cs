using Gum.Collections;
using Gum.Infra;
using System;
using System.Diagnostics;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class StructConstructorSymbol : IFuncSymbol
    {
        SymbolFactory factory;
        StructSymbol outer;
        StructConstructorDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode? ISymbolNode.GetOuter() => outer;
        IDeclSymbolNode? ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        IFuncSymbol IFuncSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        internal StructConstructorSymbol(SymbolFactory factory, StructSymbol outer, StructConstructorDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
        }

        public StructConstructorSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeStructConstructor(appliedOuter, decl);
        }

        public IFuncDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public StructSymbol GetOuter()
        {
            return outer;
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return default;
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public int GetParameterCount()
        {
            return decl.GetParameterCount();
        }

        public FuncParameter GetParameter(int index)
        {
            var parameter = decl.GetParameter(index);
            var typeEnv = GetTypeEnv();

            return parameter.Apply(typeEnv);
        }        

        public ITypeSymbol? GetOuterType()
        {
            return outer;
        }
        
    }
}