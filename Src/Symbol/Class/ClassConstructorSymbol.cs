using Citron.Collections;
using Citron.Infra;
using System;

namespace Citron.Symbol
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

        public IType GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }

        public ITypeSymbol? GetOuterType()
        {
            return outer;
        }
    }
}