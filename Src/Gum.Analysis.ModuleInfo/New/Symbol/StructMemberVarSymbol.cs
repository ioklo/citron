using System;
using Gum.Collections;

using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.Analysis
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

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
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
            var name = RItemFactory.MakeName(decl.GetName());

            return new R.Path.Nested(outerPath, name, R.ParamHash.None, default);
        }
    }
}