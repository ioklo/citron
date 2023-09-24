using Citron.Infra;
using Citron.Collections;
using System;

namespace Citron.Symbol
{
    public interface ISymbolQueryResultVisitor<out TResult>
    {   
        TResult VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result);

        TResult VisitNamespace(SymbolQueryResult.Namespace result);
        TResult VisitGlobalFuncs(SymbolQueryResult.GlobalFuncs result);

        TResult VisitClass(SymbolQueryResult.Class result);
        TResult VisitClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs result);
        TResult VisitClassMemberVar(SymbolQueryResult.ClassMemberVar result);

        TResult VisitStruct(SymbolQueryResult.Struct result);
        TResult VisitStructMemberFuncs(SymbolQueryResult.StructMemberFuncs result);
        TResult VisitStructMemberVar(SymbolQueryResult.StructMemberVar result);

        TResult VisitEnum(SymbolQueryResult.Enum result);
        TResult VisitEnumElem(SymbolQueryResult.EnumElem result);
        TResult VisitEnumElemMemberVar(SymbolQueryResult.EnumElemMemberVar result);

        TResult VisitLambdaMemberVar(SymbolQueryResult.LambdaMemberVar result);
        TResult VisitTupleMemberVar(SymbolQueryResult.TupleMemberVar result);
    }

    // Error/NotFound/Value
    public abstract record class SymbolQueryResult
    {
        public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
            where TVisitor : struct, ISymbolQueryResultVisitor<TResult>;

        // 여러 함수들을 돌려주는 것은 이 에러가 아니다. 타입 - 함수 이런식으로 겹치는 경우를 말한다
        public record class MultipleCandidatesError(ImmutableArray<SymbolQueryResult> Results) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitMultipleCandidatesError(this);
        }

        // 검색으로 나올 수 있는 종류들
        public record class Namespace(NamespaceSymbol Symbol) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitNamespace(this);
        }

        public record class GlobalFuncs(ImmutableArray<(ISymbolNode Outer, GlobalFuncDeclSymbol DeclSymbol)> OuterAndDeclSymbols) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitGlobalFuncs(this);
        }

        public record class Class(Func<ImmutableArray<IType>, ClassSymbol> ClassConstructor) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClass(this);
        }

        public record class ClassMemberFuncs(ImmutableArray<(ISymbolNode Outer, ClassMemberFuncDeclSymbol DeclSymbol)> OuterAndDeclSymbols) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberFuncs(this);
        }

        public record class ClassMemberVar(ClassMemberVarSymbol Symbol) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitClassMemberVar(this);
        }

        public record class Struct(Func<ImmutableArray<IType>, StructSymbol> StructConstructor) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStruct(this);
        }

        public record class StructMemberFuncs(ImmutableArray<(ISymbolNode Outer, StructMemberFuncDeclSymbol DeclSymbol)> OuterAndDeclSymbols) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberFuncs(this);
        }

        public record class StructMemberVar(StructMemberVarSymbol Symbol) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitStructMemberVar(this);
        }

        public record class Enum(Func<ImmutableArray<IType>, EnumSymbol> EnumConstructor) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnum(this);
        }

        public record class EnumElem(EnumElemSymbol Symbol) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElem(this);
        }

        public record class EnumElemMemberVar(EnumElemMemberVarSymbol Symbol) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitEnumElemMemberVar(this);
        }
        
        public record class LambdaMemberVar(LambdaMemberVarSymbol Symbol) : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitLambdaMemberVar(this);
        }

        // 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
        public record class TupleMemberVar() : SymbolQueryResult
        {
            public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTupleMemberVar(this);
        }
        // public record class TypeVar : SymbolQueryResult;
    }
}