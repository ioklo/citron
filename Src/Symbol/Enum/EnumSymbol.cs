using System;
using Citron.Collections;

using Citron.Module;
using Pretune;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class EnumSymbol : ITypeSymbol
    {
        SymbolFactory factory;

        ISymbolNode outer;
        EnumDeclSymbol decl;
        ImmutableArray<ITypeSymbol> typeArgs;

        TypeEnv typeEnv;

        internal EnumSymbol(SymbolFactory factory, ISymbolNode outer, EnumDeclSymbol decl, ImmutableArray<ITypeSymbol> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
            this.typeArgs = typeArgs;

            var outerTypeEnv = outer.GetTypeEnv();
            this.typeEnv = outerTypeEnv.AddTypeArgs(typeArgs);
        }

        public EnumSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeEnum(appliedOuter, decl, appliedTypeArgs);
        }

        //
        public SymbolQueryResult QueryMember(Name memberName, int typeParamCount) 
        {
            // shortcut
            if (typeParamCount != 0)
                return SymbolQueryResults.NotFound;

            var elemDecl = decl.GetElem(memberName);
            if (elemDecl == null) return SymbolQueryResults.NotFound;

            var elem = factory.MakeEnumElem(this, elemDecl);

            return new SymbolQueryResult.EnumElem(elem);
        }

        public EnumElemSymbol? GetElement(string name)
        {
            var elemDecl = decl.GetElem(new Name.Normal(name));
            if (elemDecl == null) return null;

            return factory.MakeEnumElem(this, elemDecl);
        }

        public ITypeSymbol? GetMemberType(Name memberName, ImmutableArray<ITypeSymbol> typeArgs) 
        {
            // shortcut
            if (typeArgs.Length != 0)
                return null;

            var elemDecl = decl.GetElem(memberName);
            if (elemDecl == null) return null;

            return factory.MakeEnumElem(this, elemDecl);
        }
        
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => decl;
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);

        public ITypeDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }        
        
        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        public ITypeSymbol GetTypeArg(int index)
        {
            return typeArgs[index];
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitEnum(this);
        }

        public int GetElemCount()
        {
            return decl.GetElemCount();
        }

        public EnumElemSymbol GetElement(int index)
        {
            var elemDecl = decl.GetElement(index);
            return factory.MakeEnumElem(this, elemDecl);
        }
    }
}
