using System;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using System.Collections.Immutable;

namespace Gum.IR0
{
    // 분석 중에 나타나는 Identifier들의 정보
    abstract class IdentifierResult
    {   
    }
    class NotFoundErrorIdentifierResult : IdentifierResult
    {
        public static readonly NotFoundErrorIdentifierResult Instance = new NotFoundErrorIdentifierResult();
        private NotFoundErrorIdentifierResult() { }
    }

    // TODO: 추후 에러 메세지를 자세하게 만들게 하기 위해 singleton이 아니게 될 수 있다
    class MultipleCandiatesErrorIdentifierResult : IdentifierResult
    {
        public static readonly MultipleCandiatesErrorIdentifierResult Instance = new MultipleCandiatesErrorIdentifierResult();
        private MultipleCandiatesErrorIdentifierResult() { }
    }
    
    class ExpIdentifierResult  : IdentifierResult
    {
        public Exp Exp { get; }
        public TypeValue TypeValue { get; }

        public ExpIdentifierResult(Exp exp, TypeValue typeValue)
        {
            Exp = exp;
            TypeValue = typeValue;
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
    class FuncIdentifierResult : IdentifierResult
    {
        public FuncValue FuncValue { get; }
        public FuncIdentifierResult(FuncValue funcValue)
        {
            FuncValue = funcValue;
        }
    }

    // T
    class TypeIdentifierResult : IdentifierResult
    {
        // 타입을 가리키는 레퍼런스
        public NormalTypeValue TypeValue { get; }
        public TypeIdentifierResult(NormalTypeValue typeValue)
        {
            TypeValue = typeValue;
        }
    }

    // First => E.First
    class EnumElemIdentifierResult : IdentifierResult
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
