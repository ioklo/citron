using Citron.Collections;
using Citron.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.Infra;

namespace Citron.Symbol
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

        public IType GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public IType GetDeclType()
        {
            var declType = GetDeclType();
            var typeEnv = GetTypeEnv();

            return declType.Apply(typeEnv);
        }
    }
}
