using Citron.Collections;
using Citron.Infra;

namespace Citron.Analysis
{
    public class TupleMemberVarSymbol : ISymbolNode
    {
        SymbolFactory factory;
        IHolder<TupleSymbol> outerHolder;

        ITypeSymbol declType;
        string? name;
        int index;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode? ISymbolNode.GetOuter() => GetOuter();

        internal TupleMemberVarSymbol(SymbolFactory factory, IHolder<TupleSymbol> outerHolder, ITypeSymbol declType, string? name, int index)
        {
            this.factory = factory;
            this.outerHolder = outerHolder;

            this.declType = declType;
            this.name = name;
            this.index = index;
        }

        public ITypeSymbol GetDeclType()
        {
            return declType;
        }

        public string GetName()
        {
            if (name == null)
                return $"Item{index}";

            return name;
        }

        public TupleMemberVarSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outerHolder.GetValue().Apply(typeEnv);
            var appliedDeclType = declType.Apply(typeEnv);
            return factory.MakeTupleMemberVar(new Holder<TupleSymbol>(appliedOuter), appliedDeclType, name, index);
        }

        public TupleSymbol GetOuter()
        {
            return outerHolder.GetValue();
        }

        public IDeclSymbolNode? GetDeclSymbolNode()
        {
            return null;
        }

        public TypeEnv GetTypeEnv()
        {
            return outerHolder.GetValue().GetTypeEnv();
        }

        public ITypeSymbol GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }
    }
}
