using System;

using S = Gum.Syntax;
using M = Gum.CompileTime;

namespace Gum.IR0
{
    // 분석 중에 나타나는 Identifier들의 정보
    abstract class IdentifierInfo
    {   
    }

    // 내부 글로벌 변수, x
    class InternalGlobalVarInfo : IdentifierInfo
    {
        public M.Name Name { get; }
        public TypeValue TypeValue { get; }

        public InternalGlobalVarInfo(M.Name name, TypeValue typeValue) { Name = name; TypeValue = typeValue; }
    }

    // () => { x }에서 x
    class LocalVarOutsideLambdaInfo : IdentifierInfo
    {
        public Name Name { get => localVarInfo.Name; }
        public TypeValue TypeValue { get => localVarInfo.TypeValue; }

        LocalVarInfo localVarInfo;

        public LocalVarOutsideLambdaInfo(LocalVarInfo localVarInfo)
        {
            this.localVarInfo = localVarInfo;
        }
    }

    class LocalVarInfo : IdentifierInfo
    {
        public string Name { get; }
        public TypeValue TypeValue { get; }

        public LocalVarInfo(string name, TypeValue typeValue)
        {
            Name = name;
            TypeValue = typeValue;
        }
    }

    // x => e.x
    class EnumFieldInfo : IdentifierInfo
    {
        public string Name { get; }
        public EnumFieldInfo(string name) { Name = name; }
    }
    
    // x => this.x
    class ThisMemberInfo : IdentifierInfo
    {   
        public M.Name MemberName { get; }
        public ThisMemberInfo(M.Name memberName)
        {
            MemberName = memberName;
        }
    }

    // f 
    class FuncInfo : IdentifierInfo
    {
        public FuncValue FuncValue { get; }
        public FuncInfo(FuncValue funcValue)
        {
            FuncValue = funcValue;
        }
    }

    // T
    class TypeInfo : IdentifierInfo
    {
        // 타입을 가리키는 레퍼런스
        public NormalTypeValue TypeValue { get; }
        public TypeInfo(NormalTypeValue typeValue)
        {
            TypeValue = typeValue;
        }
    }

    // F => E.F
    class EnumElemInfo : IdentifierInfo
    {
        public NormalTypeValue EnumTypeValue { get; }
        public M.Name Name { get => throw new NotImplementedException();  }
        public bool IsStandalone { get; }

        public EnumElemInfo(NormalTypeValue enumTypeValue, bool bStandalone)
        {
            EnumTypeValue = enumTypeValue;
            IsStandalone = bStandalone;
        }
    }
}
