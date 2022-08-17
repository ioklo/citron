using Citron.Infra;
using Citron.Collections;

using M = Citron.Module;
using R = Citron.IR0;
using Citron.Analysis;
using S = Citron.Syntax;
using System;

namespace Citron.Analysis
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

        // HasExplicitInstance: x.F 처럼 x가 명시적으로 있는 경우 true, F 처럼 this.F 나 C.F 를 암시적으로 나타낸 경우라면 false, C.F는 명시적이지만 인스턴스가 아니므로 false
        // ExplicitInstance: HasExplicitInstance가 true일때만 의미가 있다

        // C.F => HasExplicitInstance: true, null
        // x.F => HasExplicitInstance: true, "x"
        // F   => HasExplicitInstance: false, null
        public record ClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch, bool HasExplicitInstance, R.Loc? ExplicitInstance) : ExpResult;
        public record StructMemberFuncs(SymbolQueryResult.StructMemberFuncs QueryResult, ImmutableArray<ITypeSymbol> TypeArgsForMatch, bool HasExplicitInstance, R.Loc? ExplicitInstance) : ExpResult;
            
        public record Exp(R.Exp Result) : ExpResult;
        public record Loc(R.Loc Result, ITypeSymbol TypeSymbol) : ExpResult;
    }
}
