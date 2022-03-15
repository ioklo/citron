using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Analysis
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

        public ITypeSymbol GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }

        public TypeEnv GetTypeEnv()
        {
            throw new UnreachableCodeException();
        }
    }
}
