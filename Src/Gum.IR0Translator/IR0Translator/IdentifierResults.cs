﻿using System;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Collections;
using System.Diagnostics;
using Pretune;
using Citron.Analysis;

namespace Gum.IR0Translator
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
            // TODO: 추후 에러 메세지를 자세하게 만들게 하기 위해 singleton이 아니게 될 수 있다
            public record MultipleCandiates : Error
            {
                public static readonly MultipleCandiates Instance = new MultipleCandiates();
                MultipleCandiates() { }
            }

            public record VarWithTypeArg : Error
            {
                public static readonly VarWithTypeArg Instance = new VarWithTypeArg();
                VarWithTypeArg() { }
            }

            public record CantGetStaticMemberThroughInstance : Error
            {
                public static readonly CantGetStaticMemberThroughInstance Instance = new CantGetStaticMemberThroughInstance();
                CantGetStaticMemberThroughInstance() { }
            }

            public record CantGetTypeMemberThroughInstance : Error
            {
                public static readonly CantGetTypeMemberThroughInstance Instance = new CantGetTypeMemberThroughInstance();
                CantGetTypeMemberThroughInstance() { }
            }

            public record CantGetInstanceMemberThroughType : Error
            {
                public static readonly CantGetInstanceMemberThroughType Instance = new CantGetInstanceMemberThroughType();
                CantGetInstanceMemberThroughType() { }
            }

            public record FuncCantHaveMember : Error
            {
                public static readonly FuncCantHaveMember Instance = new FuncCantHaveMember();
                FuncCantHaveMember() { }
            }
        }

        public abstract record Valid : IdentifierResult;

        public record ThisVar : Valid
        {
            public static readonly ThisVar Instance = new ThisVar();
            ThisVar() { }
        }

        public record LocalVar(bool IsRef, ITypeSymbol TypeSymbol, string VarName) : Valid;
        public record LocalVarOutsideLambda(bool IsRef, ITypeSymbol TypeSymbol, string VarName) : Valid;
        public record GlobalVar(bool IsRef, ITypeSymbol TypeSymbol, string VarName) : Valid;

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
