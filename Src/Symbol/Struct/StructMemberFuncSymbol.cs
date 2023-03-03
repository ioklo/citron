using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Diagnostics;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public partial class StructMemberFuncSymbol : IFuncSymbol, ICyclicEqualityComparableClass<StructMemberFuncSymbol>
    {
        SymbolFactory factory;
        StructSymbol outer;
        StructMemberFuncDeclSymbol decl;
        ImmutableArray<IType> typeArgs;
        TypeEnv typeEnv; 

        IFuncSymbol IFuncSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();

        internal StructMemberFuncSymbol(SymbolFactory factory, StructSymbol outer, StructMemberFuncDeclSymbol decl, ImmutableArray<IType> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
            this.typeArgs = typeArgs;

            var outerTypeEnv = outer.GetTypeEnv();
            this.typeEnv = outerTypeEnv.AddTypeArgs(typeArgs);
        }

        SymbolQueryResult ISymbolNode.QueryMember(Name name, int explicitTypeArgCount)
        {
            return SymbolQueryResults.NotFound;
        }

        public int GetParameterCount()
        {
            return decl.GetParameterCount();
        }

        public FuncParameter GetParameter(int index)
        {
            var parameter = decl.GetParameter(index);
            return parameter.Apply(typeEnv);
        }

        public StructMemberFuncSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeStructMemberFunc(appliedOuter, decl, appliedTypeArgs);
        }

        public IFuncDeclSymbol GetDeclSymbolNode()
        {
            return decl;
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

        public ITypeSymbol? GetOuterType()
        {
            return outer;
        }

        public FuncReturn GetReturn()
        {
            var @return = decl.GetReturn();
            return @return.Apply(typeEnv);
        }

        public bool IsStatic()
        {
            return decl.IsStatic();
        }

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is StructMemberFuncSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncSymbol>.CyclicEquals(IFuncSymbol other, ref CyclicEqualityCompareContext context)
            => other is StructMemberFuncSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<StructMemberFuncSymbol>.CyclicEquals(StructMemberFuncSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(StructMemberFuncSymbol other, ref CyclicEqualityCompareContext context)
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

        public void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ISymbolNodeVisitor
        {
            visitor.VisitStructMemberFunc(this);
        }
    }
}