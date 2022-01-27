using Gum.Collections;
using System;

using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class ClassConstructorSymbol : IFuncSymbol
    {
        SymbolFactory factory;
        ClassSymbol outer;
        ClassConstructorDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        IFuncSymbol IFuncSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode? ISymbolNode.GetOuter() => GetOuter();

        internal ClassConstructorSymbol(SymbolFactory factory, ClassSymbol outer, ClassConstructorDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
        }

        public int GetParameterCount()
        {
            return decl.GetParameterCount();
        }

        public FuncParameter GetParameter(int index)
        {
            var typeEnv = outer.GetTypeEnv();
            var param = decl.GetParameter(index);
            return param.Apply(typeEnv);
        }

        public ClassSymbol GetOuter()
        {
            return outer;
        }

        public IFuncDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public ClassConstructorSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeClassConstructor(appliedOuter, decl);
        }        

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return default;
        }

        public ITypeSymbol? GetOuterType()
        {
            return outer;
        }
    }
}