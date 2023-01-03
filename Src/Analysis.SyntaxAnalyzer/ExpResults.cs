using Citron.Infra;
using Citron.Collections;

using M = Citron.Module;
using R = Citron.IR0;
using Citron.Analysis;
using S = Citron.Syntax;
using System;

namespace Citron.Analysis
{   
    abstract record class ExpResult
    {
        public record class Namespace : ExpResult;

        // include EnumElem
        public record Class(ClassSymbol Symbol) : ExpResult;
        public record class Struct(StructSymbol Symbol) : ExpResult;
        public record class Enum(EnumSymbol Symbol) : ExpResult;
        public record class EnumElem(EnumElemSymbol Symbol) : ExpResult; 

        // TypeArgsForMatch: partial
        public record class GlobalFuncs(SymbolQueryResult.GlobalFuncs QueryResult, ImmutableArray<IType> TypeArgsForMatch) : ExpResult;

        // HasExplicitInstance: x.F 처럼 x가 명시적으로 있는 경우 true, F 처럼 this.F 나 C.F 를 암시적으로 나타낸 경우라면 false, C.F는 명시적이지만 인스턴스가 아니므로 false
        // ExplicitInstance: HasExplicitInstance가 true일때만 의미가 있다

        // C.F => HasExplicitInstance: true, null
        // x.F => HasExplicitInstance: true, "x"
        // F   => HasExplicitInstance: false, null
        public record ClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs QueryResult, ImmutableArray<IType> TypeArgsForMatch, bool HasExplicitInstance, R.Loc? ExplicitInstance) : ExpResult;
        public record class StructMemberFuncs(SymbolQueryResult.StructMemberFuncs QueryResult, ImmutableArray<IType> TypeArgsForMatch, bool HasExplicitInstance, R.Loc? ExplicitInstance) : ExpResult;
            
        public record class Exp(R.Exp Result) : ExpResult;
        public record class Loc(R.Loc Result, ITypeSymbol TypeSymbol) : ExpResult;
    }
}
