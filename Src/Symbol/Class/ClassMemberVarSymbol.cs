using Citron.Collections;
using Citron.Infra;
using System;

namespace Citron.Symbol
{    
    public class ClassMemberVarSymbol : ISymbolNode, ICyclicEqualityComparableClass<ClassMemberVarSymbol>
    {
        SymbolFactory factory;
        ClassSymbol outer;
        ClassMemberVarDeclSymbol decl;
        TypeEnv typeEnv;

        // for return type covariance
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode? ISymbolNode.GetOuter() => GetOuter();

        internal ClassMemberVarSymbol(SymbolFactory factory, ClassSymbol outer, ClassMemberVarDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;

            typeEnv = outer.GetTypeEnv();
        }

        public ClassMemberVarSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeClassMemberVar(appliedOuter, decl);
        }        

        public IDeclSymbolNode GetDeclSymbolNode()
        {
            return decl;
        }

        public ClassSymbol GetOuter()
        {
            return outer;
        }
        
        public TypeEnv GetTypeEnv()
        {
            return typeEnv;
        }

        public IType GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }

        public IType GetDeclType()
        {
            var declType = decl.GetDeclType();
            var typeEnv = GetTypeEnv();

            return declType.Apply(typeEnv);
        }

        public bool IsStatic()
        {
            return decl.IsStatic();
        }

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ClassMemberVarSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ClassMemberVarSymbol>.CyclicEquals(ClassMemberVarSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(ClassMemberVarSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!context.CompareClass(decl, other.decl))
                return false;

            return true;
        }

        public void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, ISymbolNodeVisitor
        {
            visitor.VisitClassMemberVar(this);
        }
    }
}
