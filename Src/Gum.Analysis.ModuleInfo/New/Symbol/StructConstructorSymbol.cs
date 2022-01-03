using Gum.Collections;
using System;
using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.Analysis
{
    public class StructConstructorSymbol : ISymbolNode
    {
        SymbolFactory factory;
        StructSymbol outer;
        StructConstructorDeclSymbol decl;

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

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public StructSymbol GetOuter()
        {
            return outer;
        }

        public ImmutableArray<ITypeSymbolNode> GetTypeArgs()
        {
            return default;
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public M.NormalTypeId MakeChildTypeId(M.Name name, ImmutableArray<M.TypeId> typeArgs)
        {
            throw new InvalidOperationException();
        }

        public R.Path.Normal MakeRPath()
        {
            var outerPath = outer.MakeRPath();
            return decl.MakeRPath(outerPath);
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

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode? ISymbolNode.GetOuter() => outer;
    }
}