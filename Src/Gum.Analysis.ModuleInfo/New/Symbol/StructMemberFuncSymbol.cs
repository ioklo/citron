using Gum.Collections;
using Gum.Infra;
using System;
using System.Diagnostics;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public class StructMemberFuncSymbol : IFuncSymbol
    {
        SymbolFactory factory;
        StructSymbol outer;
        StructMemberFuncDeclSymbol decl;
        ImmutableArray<ITypeSymbol> typeArgs;
        TypeEnv typeEnv;

        IFuncSymbol IFuncSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();

        internal StructMemberFuncSymbol(SymbolFactory factory, StructSymbol outer, StructMemberFuncDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
            this.typeArgs = typeArgs;

            var outerTypeEnv = outer.GetTypeEnv();
            this.typeEnv = outerTypeEnv.AddTypeArgs(typeArgs);
        }

        public int GetParameterCount()
        {
            return decl.GetParameterCount();
        }

        public FuncParameter GetParameter(int index)
        {
            var parameter = decl.GetParameter(index);
            return parameter.Apply(typeEnv);
        }

        public StructMemberFuncSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeStructMemberFunc(appliedOuter, decl, appliedTypeArgs);
        }

        public IFuncDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            throw new NotImplementedException();
        }

        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        public ITypeSymbol? GetOuterType()
        {
            return outer;
        }

        public FuncReturn GetReturn()
        {
            var @return = decl.GetReturn();
            return @return.Apply(typeEnv);
        }

        public bool IsStatic()
        {
            return decl.IsStatic();
        }
    }
}