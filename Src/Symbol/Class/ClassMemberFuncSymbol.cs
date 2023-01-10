using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Symbol
{
    // X<int>.Y<short>.F_T_int_int<S>
    public class ClassMemberFuncSymbol : IFuncSymbol, ICyclicEqualityComparableClass<ClassMemberFuncSymbol>
    {
        SymbolFactory factory;

        // X<int>.Y<short>
        ClassSymbol outer;

        // F_int_int
        ClassMemberFuncDeclSymbol decl;

        ImmutableArray<IType> typeArgs;

        // cached typeEnv
        TypeEnv typeEnv;

        // for return type covariance        
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        IFuncSymbol IFuncSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);        

        internal ClassMemberFuncSymbol(SymbolFactory factory, ClassSymbol outer, ClassMemberFuncDeclSymbol decl, ImmutableArray<IType> typeArgs)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
            this.typeArgs = typeArgs;
            this.typeEnv = outer.GetTypeEnv().AddTypeArgs(typeArgs);
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

        public FuncReturn GetReturn()
        {
            var ret = decl.GetReturn();
            return ret.Apply(typeEnv);
        }        

        public ClassMemberFuncSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply(typeEnv));
            return factory.MakeClassMemberFunc(appliedOuter, decl, appliedTypeArgs);
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

        public bool IsStatic()
        {
            return decl.IsStatic();
        }

        public ITypeSymbol? GetOuterType()
        {
            return outer;
        }

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ClassMemberFuncSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<IFuncSymbol>.CyclicEquals(IFuncSymbol other, ref CyclicEqualityCompareContext context)
            => other is ClassMemberFuncSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ClassMemberFuncSymbol>.CyclicEquals(ClassMemberFuncSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(ClassMemberFuncSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!context.CompareClass(decl, other.decl))
                return false;

            if (!typeArgs.CyclicEqualsClassItem(ref other.typeArgs, ref context))
                return false;

            return true;
        }

        
    }
}
