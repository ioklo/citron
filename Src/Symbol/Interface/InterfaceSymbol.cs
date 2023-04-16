using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class InterfaceSymbol : ITypeSymbol, ICyclicEqualityComparableClass<InterfaceSymbol>
    {
        SymbolFactory factory;
        ISymbolNode outer;
        InterfaceDeclSymbol decl;
        ImmutableArray<IType> typeArgs;
        TypeEnv typeEnv;        

        internal InterfaceSymbol(SymbolFactory factory, ISymbolNode outer, InterfaceDeclSymbol decl, ImmutableArray<IType> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
            this.typeArgs = typeArgs;
            this.typeEnv = outer.GetTypeEnv().AddTypeArgs(typeArgs);
        }

        public InterfaceSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeInterface(appliedOuter, decl, appliedTypeArgs);
        }

        public ITypeDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public IType? GetMemberType(Name memberName, ImmutableArray<IType> typeArgs)
        {
            throw new NotImplementedException();
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public IType GetTypeArg(int index)
        {
            return typeArgs[index];
        }

        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        public IType MakeType()
        {
            return new InterfaceType(this);
        }

        SymbolQueryResult? ISymbolNode.QueryMember(Name name, int typeParamCount)
        {
            return null;
        }

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => decl;

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is InterfaceSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ITypeSymbol>.CyclicEquals(ITypeSymbol other, ref CyclicEqualityCompareContext context)
            => other is InterfaceSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<InterfaceSymbol>.CyclicEquals(InterfaceSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(InterfaceSymbol other, ref CyclicEqualityCompareContext context)
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
            visitor.VisitInterface(this);
        }

        void ITypeSymbol.Accept<TTypeSymbolVisitor>(ref TTypeSymbolVisitor visitor)
        {
            visitor.VisitInterface(this);
        }
    }
}
