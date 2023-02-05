using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Citron.Infra;
using Citron.Collections;
using Pretune;

namespace Citron.Symbol
{
    // X<int>.Y<short>.F_T_int_int<S>

    // F<>
    [ImplementIEquatable]
    public partial class GlobalFuncSymbol : IFuncSymbol, ICyclicEqualityComparableClass<GlobalFuncSymbol>
    {
        SymbolFactory factory;
        ITopLevelSymbolNode outer;

        // F_int_int
        GlobalFuncDeclSymbol decl;
        ImmutableArray<IType> typeArgs;

        TypeEnv typeEnv;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IFuncSymbol IFuncSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();

        internal GlobalFuncSymbol(SymbolFactory factory, ITopLevelSymbolNode outer, GlobalFuncDeclSymbol funcDecl, ImmutableArray<IType> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = funcDecl;
            this.typeArgs = typeArgs;

            var outerTypeEnv = outer.GetTypeEnv();
            this.typeEnv = outerTypeEnv.AddTypeArgs(typeArgs);
        }

        SymbolQueryResult ISymbolNode.QueryMember(Name name, int explicitTypeArgCount)
        {
            return SymbolQueryResults.NotFound;
        }

        public bool IsInstanceFunc()
        {
            return false;
        }

        public FuncParameter GetParameter(int index)
        {
            var parameter = decl.GetParameter(index);
            return parameter.Apply(typeEnv);
        }

        public FuncReturn GetReturn()
        {
            var @return = decl.GetReturn();
            return @return.Apply(typeEnv);
        }

        public GlobalFuncSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeGlobalFunc(appliedOuter, decl, appliedTypeArgs);
        }
        
        public int GetParameterCount()
        {
            return decl.GetParameterCount();
        }

        public ISymbolNode? GetOuter()
        {
            return outer;
        }

        public IFuncDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        public IType GetTypeArg(int index)
        {
            return typeArgs[index];
        }

        public ITypeSymbol? GetOuterType()
        {
            return null;
        }

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is GlobalFuncSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncSymbol>.CyclicEquals(IFuncSymbol other, ref CyclicEqualityCompareContext context)
            => other is GlobalFuncSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<GlobalFuncSymbol>.CyclicEquals(GlobalFuncSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(GlobalFuncSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!context.CompareClass(decl, other.decl))
                return false;

            if (typeArgs.CyclicEqualsClassItem(ref typeArgs, ref context))
                return false;

            return true;
        }

        public void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ISymbolNodeVisitor
        {
            visitor.VisitGlobalFunc(this);
        }
    }
}
