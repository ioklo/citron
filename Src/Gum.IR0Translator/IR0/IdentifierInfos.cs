using Gum.CompileTime;
using S = Gum.Syntax;

namespace Gum.IR0
{
    // 분석 중에 나타나는 Identifier들의 정보
    abstract class IdentifierInfo
    {   
    }

    // 내부 글로벌 변수, x
    class InternalGlobalVarInfo : IdentifierInfo
    {
        public Name Name { get; }
        public TypeValue TypeValue { get; }

        public InternalGlobalVarInfo(Name name, TypeValue typeValue) { Name = name; TypeValue = typeValue; }
    }

    // () => { x }에서 x
    class LocalVarOutsideLambdaInfo : IdentifierInfo
    {
        public bool bNeedCapture { get; set; }
        public LocalVarInfo LocalVarInfo { get; }

        public LocalVarOutsideLambdaInfo(LocalVarInfo localVarInfo)
        {
            bNeedCapture = false;
            LocalVarInfo = localVarInfo;
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

    // x => T.x
    class StaticMemberInfo : IdentifierInfo
    {   
        public MemberVarValue VarValue { get; }
        public StaticMemberInfo(MemberVarValue varValue) { VarValue = varValue; }
    }

    // x => this.x
    // TODO: ThisMember라고 불리는게 나을 것 같다
    class InstanceMemberInfo : IdentifierInfo
    {
        public S.Exp ObjectExp { get; }
        public TypeValue ObjectTypeValue { get; }
        public Name VarName { get; }
        public InstanceMemberInfo(S.Exp objectExp, TypeValue objectTypeValue, Name varName)
        {
            ObjectExp = objectExp;
            ObjectTypeValue = objectTypeValue;
            VarName = varName;
        }
    }

    // f 
    class FuncInfo : IdentifierInfo
    {
        // 함수를 가리키는 레퍼런스
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
        public TypeValue.Normal TypeValue { get; }
        public TypeInfo(TypeValue.Normal typeValue)
        {
            TypeValue = typeValue;
        }
    }

    // F => E.F
    class EnumElemInfo : IdentifierInfo
    {
        public TypeValue.Normal EnumTypeValue { get; }
        public EnumElemInfo ElemInfo { get; }

        public EnumElemInfo(TypeValue.Normal enumTypeValue, EnumElemInfo elemInfo)
        {
            EnumTypeValue = enumTypeValue;
            ElemInfo = elemInfo;
        }
    }
}
