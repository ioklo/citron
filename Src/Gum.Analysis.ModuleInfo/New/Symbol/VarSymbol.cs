using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Analysis
{
    public class VarSymbol : ISymbolNode
    {
        internal VarSymbol() { }

        public ISymbolNode Apply(TypeEnv typeEnv)
        {
            return this;
        }

        public IDeclSymbolNode? GetDeclSymbolNode()
        {
            return null;
        }

        public ISymbolNode? GetOuter()
        {
            return null;
        }

        public ImmutableArray<ITypeSymbol> GetTypeArgs()
        {
            throw new UnreachableCodeException();
        }

        public TypeEnv GetTypeEnv()
        {
            throw new UnreachableCodeException();
        }
    }
}
