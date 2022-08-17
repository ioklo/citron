using Citron.Collections;
using Citron.Infra;
using System;

using Citron.Module;

namespace Citron.Symbol
{
    // int?
    public class NullableSymbol : ITypeSymbol
    {   
        SymbolFactory factory;
        ITypeSymbol innerType;

        IDeclSymbolNode? ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        internal NullableSymbol(SymbolFactory factory, ITypeSymbol innerType)
        {
            this.factory = factory;
            this.innerType = innerType;
        }

        public ITypeSymbol GetInnerType()
        {
            return innerType;
        }

        // T? => int?
        public NullableSymbol Apply(TypeEnv typeEnv)
        {
            var appliedInnerType = innerType.Apply(typeEnv);
            return factory.MakeNullable(appliedInnerType);
        }

        public ITypeDeclSymbol? GetDeclSymbolNode()
        {
            return null;
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitNullable(this);
        }

        public SymbolQueryResult QueryMember(Name memberName, int typeParamCount)
        {
            throw new UnreachableCodeException();
        }

        public ISymbolNode? GetOuter()
        {
            return null;
        }

        public TypeEnv GetTypeEnv()
        {
            return default;
        }

        public ITypeSymbol GetTypeArg(int index)
        {
            throw new NotImplementedException();
        }
    }
}
