using System;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using System.Collections.Immutable;

namespace Gum.IR0Translator
{
    // 분석 중에 나타나는 Identifier들의 정보
    abstract class IdentifierResult
    {   
    }

    // Error, Valid, NotFound
    abstract class ErrorIdentifierResult : IdentifierResult { }
    abstract class ValidIdentifierResult : IdentifierResult { }

    class NotFoundIdentifierResult : IdentifierResult
    {
        public static readonly NotFoundIdentifierResult Instance = new NotFoundIdentifierResult();
        private NotFoundIdentifierResult() { }
    }

    // TODO: 추후 에러 메세지를 자세하게 만들게 하기 위해 singleton이 아니게 될 수 있다
    class MultipleCandiatesErrorIdentifierResult : ErrorIdentifierResult
    {
        public static readonly MultipleCandiatesErrorIdentifierResult Instance = new MultipleCandiatesErrorIdentifierResult();
        private MultipleCandiatesErrorIdentifierResult() { }
    }

    class VarWithTypeArgErrorIdentifierResult : ErrorIdentifierResult
    {
        public static readonly VarWithTypeArgErrorIdentifierResult Instance = new VarWithTypeArgErrorIdentifierResult();
        VarWithTypeArgErrorIdentifierResult() { }
    }

    class CantGetStaticMemberThroughInstanceIdentifierResult : ErrorIdentifierResult
    {
        public static readonly CantGetStaticMemberThroughInstanceIdentifierResult Instance = new CantGetStaticMemberThroughInstanceIdentifierResult();
        private CantGetStaticMemberThroughInstanceIdentifierResult() { }
    }

    class CantGetTypeMemberThroughInstanceIdentifierResult : ErrorIdentifierResult
    {
        public static readonly CantGetTypeMemberThroughInstanceIdentifierResult Instance = new CantGetTypeMemberThroughInstanceIdentifierResult();
        private CantGetTypeMemberThroughInstanceIdentifierResult() { }
    }

    class CantGetInstanceMemberThroughTypeIdentifierResult : ErrorIdentifierResult
    {
        public static readonly CantGetInstanceMemberThroughTypeIdentifierResult Instance = new CantGetInstanceMemberThroughTypeIdentifierResult();
        private CantGetInstanceMemberThroughTypeIdentifierResult() { }
    }

    class FuncCantHaveMemberErrorIdentifierResult : ErrorIdentifierResult
    {
        public static readonly FuncCantHaveMemberErrorIdentifierResult Instance = new FuncCantHaveMemberErrorIdentifierResult();
        private FuncCantHaveMemberErrorIdentifierResult() { }
    } 

    abstract record LambdaCapture;
    record NoneLambdaCapture : LambdaCapture { public static readonly NoneLambdaCapture Instance = new NoneLambdaCapture(); private NoneLambdaCapture() { } }
    record ThisLambdaCapture : LambdaCapture { public static readonly ThisLambdaCapture Instance = new ThisLambdaCapture(); private ThisLambdaCapture() { } }
    record LocalLambdaCapture(string Name, TypeValue Type) : LambdaCapture;

    class ExpIdentifierResult : ValidIdentifierResult
    {
        public R.Exp Exp { get; }
        public TypeValue TypeValue { get; }
        public LambdaCapture LambdaCapture { get; }

        public ExpIdentifierResult(R.Exp exp, TypeValue typeValue, LambdaCapture lambdaCapture)
        {
            Exp = exp;
            TypeValue = typeValue;
            LambdaCapture = lambdaCapture;
        }
    }

    //// 내부 글로벌 변수, x
    //class InternalGlobalVarInfo : IdentifierInfo
    //{
    //    public M.Name Name { get; }
    //    public TypeValue TypeValue { get; }

    //    public InternalGlobalVarInfo(M.Name name, TypeValue typeValue) { Name = name; TypeValue = typeValue; }
    //}

    //// () => { x }에서 x
    //class LocalVarOutsideLambdaInfo : IdentifierInfo
    //{
    //    public Name Name { get => localVarInfo.Name; }
    //    public TypeValue TypeValue { get => localVarInfo.TypeValue; }

    //    LocalVarInfo localVarInfo;

    //    public LocalVarOutsideLambdaInfo(LocalVarInfo localVarInfo)
    //    {
    //        this.localVarInfo = localVarInfo;
    //    }
    //}

    //class LocalVarInfo : IdentifierInfo
    //{
    //    public string Name { get; }
    //    public TypeValue TypeValue { get; }

    //    public LocalVarInfo(string name, TypeValue typeValue)
    //    {
    //        Name = name;
    //        TypeValue = typeValue;
    //    }
    //}

    //// x => e.x ?? 
    //class EnumFieldInfo : IdentifierInfo
    //{
    //    public string Name { get; }
    //    public EnumFieldInfo(string name) { Name = name; }
    //}
    
    //// x => this.x
    //class ThisMemberInfo : IdentifierInfo
    //{   
    //    public M.Name MemberName { get; }
    //    public ThisMemberInfo(M.Name memberName)
    //    {
    //        MemberName = memberName;
    //    }
    //}

    //// F
    class FuncIdentifierResult : ValidIdentifierResult
    {
        public FuncValue FuncValue { get; }
        public FuncIdentifierResult(FuncValue funcValue)
        {
            FuncValue = funcValue;
        }
    }

    // T
    class TypeIdentifierResult : ValidIdentifierResult
    {
        // 타입을 가리키는 레퍼런스
        public TypeValue TypeValue { get; }
        public TypeIdentifierResult(TypeValue typeValue)
        {
            TypeValue = typeValue;
        }
    }

    // First => E.First
    class EnumElemIdentifierResult : ValidIdentifierResult
    {
        public NormalTypeValue EnumTypeValue { get; }
        public M.Name Name { get => throw new NotImplementedException();  }
        public ImmutableArray<M.EnumElemFieldInfo> FieldInfos { get; }
        public bool IsStandalone { get => FieldInfos.IsEmpty; }

        public EnumElemIdentifierResult(NormalTypeValue enumTypeValue, ImmutableArray<M.EnumElemFieldInfo> fieldInfos)
        {
            EnumTypeValue = enumTypeValue;
            FieldInfos = fieldInfos;
        }
    }
}
