using Citron.Infra;
using Citron.Collections;
using System;

namespace Citron.Symbol
{
    // Error/NotFound/Value
    public abstract record class SymbolQueryResult
    {   
        public record class Error : SymbolQueryResult
        {
            public record class VarWithTypeArg : Error
            {
                internal VarWithTypeArg() { }
            }

            public record class MultipleCandidates : Error
            {
                internal MultipleCandidates() { }
            }
        }

        public record class NotFound : SymbolQueryResult
        {   
            internal NotFound() { }
        }

        // 검색으로 나올 수 있는 종류들
        public abstract record class Valid : SymbolQueryResult;
        
        public record class GlobalFuncs(ImmutableArray<DeclAndConstructor<GlobalFuncDeclSymbol, GlobalFuncSymbol>> Infos) : Valid;

        public record Class(Func<ImmutableArray<IType>, ClassSymbol> ClassConstructor) : Valid;
        public record ClassMemberFuncs(ImmutableArray<DeclAndConstructor<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>> Infos) : Valid;
        public record ClassMemberVar(ClassMemberVarSymbol Var) : Valid;

        public record class Struct(Func<ImmutableArray<IType>, StructSymbol> StructConstructor) : Valid;        
        public record class StructMemberFuncs(ImmutableArray<DeclAndConstructor<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>> Infos) : Valid;
        public record class StructMemberVar(StructMemberVarSymbol Var) : Valid;

        public record class Enum(Func<ImmutableArray<IType>, EnumSymbol> EnumConstructor) : Valid;
        public record class EnumElem(EnumElemSymbol Symbol) : Valid;
        public record class EnumElemMemberVar(EnumElemMemberVarSymbol Symbol) : Valid;

        public record class Lambda(LambdaSymbol Symbol) : Valid;
        public record class LambdaMemberVar(LambdaMemberVarSymbol Symbol) : Valid;

        // 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
        public record class TupleMemberVar() : Valid; 
        // public record class TypeVar : Valid;
    }

    public static class SymbolQueryResults
    {
        public readonly static SymbolQueryResult.NotFound NotFound = new SymbolQueryResult.NotFound();
        public static class Error
        {
            public readonly static SymbolQueryResult.Error.VarWithTypeArg VarWithTypeArg = new SymbolQueryResult.Error.VarWithTypeArg();

            // 여러 함수들을 돌려주는 것은 이 에러가 아니다. 타입 - 함수 이런식으로 겹치는 경우를 말한다
            public readonly static SymbolQueryResult.Error.MultipleCandidates MultipleCandidates = new SymbolQueryResult.Error.MultipleCandidates();
        }
    }
}