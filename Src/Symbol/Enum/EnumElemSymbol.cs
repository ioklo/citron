using Citron.Collections;
using System.Diagnostics;

using Pretune;
using System;
using Citron.Infra;

namespace Citron.Symbol
{
    // S.First, S.Second(int i, short s)    
    [ImplementIEquatable]
    public partial class EnumElemSymbol : ITypeSymbol, ICyclicEqualityComparableClass<EnumElemSymbol>
    {
        SymbolFactory factory;
        EnumSymbol outer;
        EnumElemDeclSymbol decl;

        internal EnumElemSymbol(SymbolFactory factory, EnumSymbol outer, EnumElemDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
        }

        public bool IsStandalone()
        {
            return decl.IsStandalone();
        }

        public EnumElemSymbol Apply(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            return factory.MakeEnumElem(appliedOuter, decl);
        }

        public ImmutableArray<FuncParameter> GetConstructorParamTypes()
        {
            var memberVarCount = decl.GetMemberVarCount();

            var builder = ImmutableArray.CreateBuilder<FuncParameter>(memberVarCount);
            for(int i = 0; i < memberVarCount; i++)
            {
                var memberVar = decl.GetMemberVar(i);

                var typeEnv = outer.GetTypeEnv();
                var declType = memberVar.GetDeclType();
                
                var appliedDeclType = declType.Apply(typeEnv);

                builder.Add(new FuncParameter(FuncParameterKind.Default, appliedDeclType, decl.GetName())); // TODO: EnumElemFields에 ref를 지원할지
            }

            return builder.MoveToImmutable();
        }

        SymbolQueryResult ISymbolNode.QueryMember(Name memberName, int typeParamCount)
        {
            if (typeParamCount != 0) return SymbolQueryResults.NotFound;

            int memberVarCount = decl.GetMemberVarCount();

            for(int i = 0; i < memberVarCount; i++)
            {
                var varDecl = decl.GetMemberVar(i);

                if (memberName.Equals(varDecl.GetName()))
                {
                    if (typeParamCount != 0)
                        return SymbolQueryResults.Error.VarWithTypeArg;

                    var memberVar = factory.MakeEnumElemMemberVar(this, varDecl);
                    return new SymbolQueryResult.EnumElemMemberVar(memberVar);
                }
            }

            return SymbolQueryResults.NotFound;
        }

        public ITypeDeclSymbol GetDeclSymbolNode()
        {
            return decl;
        }

        public EnumSymbol GetOuter()
        {
            return outer;
        }

        public TypeEnv GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }

        public IType GetTypeArg(int index)
        {
            throw new RuntimeFatalException();
        }

        public int GetMemberVarCount()
        {
            return decl.GetMemberVarCount();
        }

        public EnumElemMemberVarSymbol GetMemberVar(int i)
        {
            var memberVarDecl = decl.GetMemberVar(i);
            return factory.MakeEnumElemMemberVar(this, memberVarDecl);
        }

        public void Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitEnumElem(this);
        }

        IType ITypeSymbol.MakeType()
        {
            return new EnumElemType(this);
        }

        IType? ITypeSymbol.GetMemberType(Name memberName, ImmutableArray<IType> typeArgs)
        {
            return null;
        }

        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        IDeclSymbolNode ISymbolNode.GetDeclSymbolNode() => GetDeclSymbolNode();
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv) => Apply(typeEnv);
        ISymbolNode? ISymbolNode.GetOuter() => GetOuter();

        bool ICyclicEqualityComparableClass<ISymbolNode>.CyclicEquals(ISymbolNode other, ref CyclicEqualityCompareContext context)
            => other is EnumElemSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<ITypeSymbol>.CyclicEquals(ITypeSymbol other, ref CyclicEqualityCompareContext context)
            => other is EnumElemSymbol otherSymbol && CyclicEquals(otherSymbol, ref context);

        bool ICyclicEqualityComparableClass<EnumElemSymbol>.CyclicEquals(EnumElemSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(EnumElemSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!context.CompareClass(decl, other.decl))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(outer), outer);
            context.SerializeRef(nameof(decl), decl);
        }
        
        void ISymbolNode.Accept<TVisitor>(ref TVisitor visitor)
        {
            visitor.VisitEnumElem(this);
        }

        void ITypeSymbol.Accept<TTypeSymbolVisitor>(ref TTypeSymbolVisitor visitor)
        {
            visitor.VisitEnumElem(this);
        }

    }
}
