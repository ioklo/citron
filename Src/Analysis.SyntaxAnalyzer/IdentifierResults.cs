using System;

using S = Citron.Syntax;
using M = Citron.Module;
using R = Citron.IR0;
using Citron.Collections;
using System.Diagnostics;
using Pretune;
using Citron.Module;

namespace Citron.Analysis
{
    // 분석 중에 나타나는 Identifier들의 정보
    abstract record class IdentifierResult
    {
        // NotFound, Error, Valid
        public record class NotFound : IdentifierResult
        {
            public static readonly NotFound Instance = new NotFound();
            NotFound() { }
        }

        public abstract record class Error : IdentifierResult 
        {
            public record class Singleton(string DebugMessage) : Error;

            // TODO: 추후 에러 메세지를 자세하게 만들게 하기 위해 singleton이 아니게 될 수 있다
            public readonly static Singleton MultipleCandiates = new Singleton("MultipleCandidates");
            public readonly static Singleton VarWithTypeArg = new Singleton("VarWithTypeArg");
            public readonly static Singleton CantGetStaticMemberThroughInstance = new Singleton("CantGetStaticMemberThroughInstance");
            public readonly static Singleton CantGetTypeMemberThroughInstance = new Singleton("CantGetTypeMemberThroughInstance");
            public readonly static Singleton CantGetInstanceMemberThroughType = new Singleton("CantGetInstanceMemberThroughType");
            public readonly static Singleton FuncCantHaveMember = new Singleton("FuncCantHaveMember");
            public readonly static Singleton CantGetThis = new Singleton("CantGetThis");
        }

        public abstract record class Valid : IdentifierResult;

        public record class ThisVar(ITypeSymbol TypeSymbol) : Valid;
        public record class LocalVar(bool IsRef, ITypeSymbol TypeSymbol, Name VarName) : Valid;
        public record class LambdaMemberVar(Func<LambdaMemberVarSymbol> SymbolConstructor) : Valid;

        public record class GlobalVar(bool IsRef, ITypeSymbol TypeSymbol, Name VarName) : Valid;

        public record class GlobalFuncs(SymbolQueryResult.GlobalFuncs QueryResult, ImmutableArray<IType> TypeArgsForMatch) : Valid;
            
        // T
        public record Class(ClassSymbol Symbol) : Valid;
        public record ClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs QueryResult, ImmutableArray<IType> TypeArgsForMatch) : Valid;
        public record ClassMemberVar(ClassMemberVarSymbol Symbol) : Valid;

        public record class Struct(StructSymbol Symbol) : Valid;
        public record class StructMemberFuncs(SymbolQueryResult.StructMemberFuncs QueryResult, ImmutableArray<IType> TypeArgsForMatch) : Valid;
        public record class StructMemberVar(StructMemberVarSymbol Symbol) : Valid;

        public record class Enum(EnumSymbol Symbol) : Valid;

        // First => E.First
        public record class EnumElem(EnumElemSymbol EnumElemSymbol) : Valid;
    }       
    
    
}
