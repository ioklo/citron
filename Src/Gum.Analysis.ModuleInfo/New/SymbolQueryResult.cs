﻿using Gum.Infra;
using Gum.Collections;
using Gum.Analysis;
using System;

namespace Gum.Analysis
{
    // Error/NotFound/Value
    public abstract record SymbolQueryResult
    {   
        public record Error : SymbolQueryResult
        {
            public record VarWithTypeArg : Error
            {
                public static readonly VarWithTypeArg Instance = new VarWithTypeArg();
                VarWithTypeArg() { }
            }

            public record MultipleCandidates : Error
            {
                public static readonly MultipleCandidates Instance = new MultipleCandidates();
                MultipleCandidates() { }
            }
        }

        public record NotFound : SymbolQueryResult
        {
            public static readonly NotFound Instance = new NotFound();
            NotFound() { }
        }

        public abstract record Valid : SymbolQueryResult;

        // ItemValue류 대신에 => 아직 정보가 부족해서 그랬다
        // public record Type(ItemValueOuter Outer, IModuleTypeDecl TypeInfo) : Valid;
        // public record Type(Func<ImmutableArray<ITypeSymbol>, ITypeSymbolNode> SymbolConstructor) : Valid;
        //public record Constructors(NormalTypeValue Outer, ImmutableArray<ConstructorDeclSymbol> ConstructorInfos) : Valid;
        //public record Funcs(ItemValueOuter Outer, ImmutableArray<IModuleFuncDecl> FuncInfos, bool IsInstanceFunc) : Valid;       
        
        // 검색으로 나올 수 있는 종류들
        public record GlobalFuncs(ImmutableArray<DeclAndConstructor<GlobalFuncDeclSymbol, GlobalFuncSymbol>> Infos) : Valid;

        public record Class(Func<ImmutableArray<ITypeSymbol>, ClassSymbol> ClassConstructor) : Valid;
        public record ClassMemberFuncs(ImmutableArray<DeclAndConstructor<ClassMemberFuncDeclSymbol, ClassMemberFuncSymbol>> Infos) : Valid;
        public record ClassMemberVar(ClassMemberVarSymbol Var) : Valid;

        public record Struct(Func<ImmutableArray<ITypeSymbol>, StructSymbol> StructConstructor) : Valid;        
        public record StructMemberFuncs(ImmutableArray<DeclAndConstructor<StructMemberFuncDeclSymbol, StructMemberFuncSymbol>> Infos) : Valid;
        public record StructMemberVar(StructMemberVarSymbol Var) : Valid;

        public record Enum(Func<ImmutableArray<ITypeSymbol>, EnumSymbol> EnumConstructor) : Valid;
        public record EnumElem(EnumElemSymbol Symbol) : Valid;
        public record EnumElemMemberVar(EnumElemMemberVarSymbol Symbol) : Valid;

        public record Lambda(LambdaSymbol Symbol) : Valid;
        public record LambdaMemberVar(LambdaMemberVarSymbol Symbol) : Valid;

        public record TupleMemberVar(TupleMemberVarSymbol Symbol) : Valid;
    }
}