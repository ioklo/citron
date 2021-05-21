using System;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Collections;
using System.Diagnostics;
using Pretune;

namespace Gum.IR0Translator
{
    // 분석 중에 나타나는 Identifier들의 정보
    abstract record IdentifierResult
    {
        // Error, Valid, NotFound
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
        
        public record LocalVar(string VarName, TypeValue TypeValue) : Valid;
        public record LocalVarOutsideLambda(string VarName, TypeValue TypeValue) : Valid;
        public record GlobalVar(string VarName, TypeValue TypeValue) : Valid;            
        public record Funcs(ItemValueOuter Outer, ImmutableArray<M.FuncInfo> FuncInfos, ImmutableArray<TypeValue> TypeArgs, bool IsInstanceFunc) : Valid;
            
        // T
        public record Type(TypeValue TypeValue) : Valid;

        // First => E.First
        public record EnumElem : Valid
        {
            public NormalTypeValue EnumTypeValue { get; }
            public M.Name Name { get => throw new NotImplementedException(); }
            public ImmutableArray<M.EnumElemFieldInfo> FieldInfos { get; }
            public bool IsStandalone { get => FieldInfos.IsEmpty; }

            public EnumElem(NormalTypeValue enumTypeValue, ImmutableArray<M.EnumElemFieldInfo> fieldInfos)
            {
                EnumTypeValue = enumTypeValue;
                FieldInfos = fieldInfos;
            }
        }
        

        public record NotFound : IdentifierResult
        {
            public static readonly NotFound Instance = new NotFound();
            NotFound() { }
        }

    }       
    
    
}
