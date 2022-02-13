using Citron.Infra;
using Citron.Collections;

using M = Citron.CompileTime;
using R = Citron.IR0;
using Citron.Analysis;
using S = Citron.Syntax;
using System;

namespace Citron.IR0Translator
{
    partial class Analyzer
    {
        abstract record ExpResult
        {
            public record Namespace : ExpResult;

            // include EnumElem
            public record Class(ClassSymbol Symbol) : ExpResult;
            public record Struct(StructSymbol Symbol) : ExpResult;
            public record Enum(EnumSymbol Symbol) : ExpResult;
            public record EnumElem(EnumElemSymbol Symbol) : ExpResult; 

            // TypeArgsForMatch: partial
            public record GlobalFuncs(SymbolQueryResult.GlobalFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch) : ExpResult;
            // bCheckInstanceForStatic: static 함수를 호출하는 위치가 선언한 타입 내부라면 체크하지 않고 넘어간다 (멤버 호출이 아닌 경우)
            public record ClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch, R.Loc? Instance, bool bCheckInstanceForStatic) : ExpResult;
            public record StructMemberFuncs(SymbolQueryResult.StructMemberFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch, R.Loc? Instance, bool bCheckInstanceForStatic) : ExpResult;
            
            public record Exp(R.Exp Result) : ExpResult;
            public record Loc(R.Loc Result, ITypeSymbol TypeSymbol) : ExpResult;
        }
    }
}
