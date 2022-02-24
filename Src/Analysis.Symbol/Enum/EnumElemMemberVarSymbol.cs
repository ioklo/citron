using Citron.Collections;
using M = Citron.CompileTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Analysis
{
    public class EnumElemMemberVarSymbol : ISymbolNode
    {
        SymbolFactory factory;
        EnumElemSymbol outer;
        EnumElemMemberVarDeclSymbol decl;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);

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

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            return default;
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public ITypeSymbol GetDeclType()
        {
            var declType = GetDeclType();
            var typeEnv = GetTypeEnv();

            return declType.Apply(typeEnv);
        }
    }
}
