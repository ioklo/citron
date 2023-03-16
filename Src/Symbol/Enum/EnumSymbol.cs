using System;
using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class EnumSymbol : ITypeSymbol, ICyclicEqualityComparableClass<EnumSymbol>
    {
        SymbolFactory factory;

        ISymbolNode outer;
        EnumDeclSymbol decl;
        ImmutableArray<IType> typeArgs;

        TypeEnv typeEnv;

        internal EnumSymbol(SymbolFactory factory, ISymbolNode outer, EnumDeclSymbol decl, ImmutableArray<IType> typeArgs)
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
        SymbolQueryResult ISymbolNode.QueryMember(Name memberName, int typeParamCount) 
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

        IType? ITypeSymbol.GetMemberType(Name memberName, ImmutableArray<IType> typeArgs) 
        {
            // shortcut
            if (typeArgs.Length != 0)
                return null;

            var elemDecl = decl.GetElem(memberName);
            if (elemDecl == null) return null;

            return ((ITypeSymbol)factory.MakeEnumElem(this, elemDecl)).MakeType();
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

        public IType GetTypeArg(int index)
        {
            return typeArgs[index];
        }

        IType ITypeSymbol.MakeType()
        {
            return new EnumType(this);
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

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is EnumSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ITypeSymbol>.CyclicEquals(ITypeSymbol other, ref CyclicEqualityCompareContext context)
            => other is EnumSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<EnumSymbol>.CyclicEquals(EnumSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(EnumSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!context.CompareClass(decl, other.decl))
                return false;

            if (!typeArgs.CyclicEqualsClassItem(ref typeArgs, ref context))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(outer), outer);
            context.SerializeRef(nameof(decl), decl);
            context.SerializeRefArray(nameof(typeArgs), typeArgs);
        }

        void ISymbolNode.Accept<TVisitor>(ref TVisitor visitor)
        {
            visitor.VisitEnum(this);
        }

        void ITypeSymbol.Accept<TTypeSymbolVisitor>(ref TTypeSymbolVisitor visitor)
        {
            visitor.VisitEnum(this);
        }
    }
}
