using Gum.Collections;
using M = Gum.CompileTime;
using R = Gum.IR0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Analysis
{
    public class EnumElemMemberVarSymbol : ISymbolNode
    {
        SymbolFactory factory;
        EnumElemSymbol outer;
        EnumElemMemberVarDeclSymbol decl;

        internal EnumElemMemberVarSymbol(SymbolFactory factory, EnumElemSymbol outer, EnumElemMemberVarDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
        }

        public EnumElemMemberVarSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeEnumElemMemberVar(appliedOuter, decl);
        }       

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
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

        public R.Path.Nested MakeRPath()
        {
            var rname = RItemFactory.MakeName(decl.GetName());
            var outerPath = outer.MakeRPath();
            return new R.Path.Nested(outerPath, rname, R.ParamHash.None, default);
        }

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        R.Path.Normal ISymbolNode.MakeRPath() => MakeRPath();
    }
}
