using System;

using S = Citron.Syntax;
using M = Citron.CompileTime;
using R = Citron.IR0;
using Citron.Collections;
using System.Diagnostics;
using Pretune;
using Citron.CompileTime;

namespace Citron.Analysis
{
    // 분석 중에 나타나는 Identifier들의 정보
    abstract record IdentifierResult
    {
        // NotFound, Error, Valid
        public record NotFound : IdentifierResult
        {
            public static readonly NotFound Instance = new NotFound();
            NotFound() { }
        }

        public abstract record Error : IdentifierResult 
        {
            public record Singleton(string DebugMessage) : Error;

            // TODO: 추후 에러 메세지를 자세하게 만들게 하기 위해 singleton이 아니게 될 수 있다
            public readonly static Singleton MultipleCandiates = new Singleton("MultipleCandidates");
            public readonly static Singleton VarWithTypeArg = new Singleton("VarWithTypeArg");
            public readonly static Singleton CantGetStaticMemberThroughInstance = new Singleton("CantGetStaticMemberThroughInstance");
            public readonly static Singleton CantGetTypeMemberThroughInstance = new Singleton("CantGetTypeMemberThroughInstance");
            public readonly static Singleton CantGetInstanceMemberThroughType = new Singleton("CantGetInstanceMemberThroughType");
            public readonly static Singleton FuncCantHaveMember = new Singleton("FuncCantHaveMember");
            public readonly static Singleton CantGetThis = new Singleton("CantGetThis");
        }

        public abstract record Valid : IdentifierResult;

        public record ThisVar(ITypeSymbol TypeSymbol) : Valid;
        public record LocalVar(bool IsRef, ITypeSymbol TypeSymbol, Name VarName) : Valid;
        public record LambdaMemberVar(Func<LambdaMemberVarSymbol> SymbolConstructor) : Valid;

        public record GlobalVar(bool IsRef, ITypeSymbol TypeSymbol, Name VarName) : Valid;

        public record GlobalFuncs(SymbolQueryResult.GlobalFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch) : Valid;
            
        // T
        public record Class(ClassSymbol Symbol) : Valid;
        public record ClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch) : Valid;
        public record ClassMemberVar(ClassMemberVarSymbol Symbol) : Valid;

        public record Struct(StructSymbol Symbol) : Valid;
        public record StructMemberFuncs(SymbolQueryResult.StructMemberFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch) : Valid;
        public record StructMemberVar(StructMemberVarSymbol Symbol) : Valid;

        public record Enum(EnumSymbol Symbol) : Valid;

        // First => E.First
        public record EnumElem(EnumElemSymbol EnumElemSymbol) : Valid;
    }       
    
    
}
