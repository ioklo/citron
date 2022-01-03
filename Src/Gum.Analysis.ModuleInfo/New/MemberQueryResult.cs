using Gum.Infra;
using Gum.Collections;
using Gum.Analysis;
using System;

namespace Gum.Analysis
{
    // Error/NotFound/Value
    public abstract record MemberQueryResult
    {   
        public record Error : MemberQueryResult
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

        public record NotFound : MemberQueryResult
        {
            public static readonly NotFound Instance = new NotFound();
            NotFound() { }
        }

        public abstract record Valid : MemberQueryResult;
        
        // ItemValue류 대신에 => 아직 정보가 부족해서 그랬다
        // public record Type(ItemValueOuter Outer, IModuleTypeDecl TypeInfo) : Valid;
        // public record Type(Func<ImmutableArray<ITypeSymbolNode>, TypeSymbol> SymbolConstructor) : Valid;
        //public record Constructors(NormalTypeValue Outer, ImmutableArray<ConstructorDeclSymbol> ConstructorInfos) : Valid;
        //public record Funcs(ItemValueOuter Outer, ImmutableArray<IModuleFuncDecl> FuncInfos, bool IsInstanceFunc) : Valid;

        // 멤버 검색으로 나올 수 있는 종류들
        public record Class(Func<ImmutableArray<ITypeSymbolNode>, ClassSymbol> ClassConstructor) : Valid;
        public record Struct(Func<ImmutableArray<ITypeSymbolNode>, StructSymbol> StructConstructor) : Valid;
        public record Enum(Func<ImmutableArray<ITypeSymbolNode>, EnumSymbol> EnumConstructor) : Valid;
        public record EnumElem(EnumElemSymbol Symbol) : Valid;
        public record EnumElemMemberVar(EnumElemMemberVarSymbol Symbol) : Valid;

        // have constructors of constructors
        public record ClassConstructors(ImmutableArray<ClassConstructorSymbol> Constructors) : Valid; // typeArgs가 없으니 확정
        public record ClassMemberFuncs(ImmutableArray<Func<ImmutableArray<ITypeSymbolNode>, ClassMemberFuncSymbol>> FuncConstructors) : Valid;
        public record ClassMemberVar(ClassMemberVarSymbol Var) : Valid;

        public record StructConstructors(ImmutableArray<StructConstructorSymbol> Constructors) : Valid;
        public record StructMemberFuncs(ImmutableArray<Func<ImmutableArray<ITypeSymbolNode>, StructMemberFuncSymbol>> FuncConstructors) : Valid;
        public record StructMemberVar(StructMemberVarSymbol Var) : Valid;
    }
}