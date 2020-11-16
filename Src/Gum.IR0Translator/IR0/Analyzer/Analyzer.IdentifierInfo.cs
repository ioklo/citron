using Gum.CompileTime;
using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Analyzer
    {
        // Identifier가 어떻게 해석될 수 있는지
        public class IdentifierInfo
        {
            public class ModuleGlobal : IdentifierInfo
            {
                public VarValue VarValue { get; }
                public TypeValue TypeValue { get; }
                internal ModuleGlobal(VarValue varValue, TypeValue typeValue) { VarValue = varValue; TypeValue = typeValue; }
            }

            public class PrivateGlobal : IdentifierInfo
            {
                public string Name { get; }
                public TypeValue TypeValue { get; }

                public PrivateGlobal(string name, TypeValue typeValue) { Name = name; TypeValue = typeValue; }
            }

            public class LocalOutsideLambda : IdentifierInfo
            {
                public LocalVarOutsideLambdaInfo Info { get; }
                public LocalOutsideLambda(LocalVarOutsideLambdaInfo info) { Info = info; }
            }

            public class Local : IdentifierInfo
            {
                public string Name { get; }
                public TypeValue TypeValue { get; }

                public Local(string name, TypeValue typeValue) { Name = name; TypeValue = typeValue; }
            }

            // x => e.x
            public class EnumField : IdentifierInfo
            {
                public string Name { get; }
                public EnumField(string name) { Name = name; }
            }

            // x => T.x
            public class StaticMember : IdentifierInfo
            {
                public (TypeValue TypeValue, S.Exp Exp)? ObjectInfo { get; }
                public VarValue VarValue { get; }
                public StaticMember((TypeValue TypeValue, S.Exp Exp)? objectInfo, VarValue varValue) { ObjectInfo = objectInfo; VarValue = varValue; }
            }

            // x => this.x
            // TODO: ThisMember라고 불리는게 나을 것 같다
            public class InstanceMember : IdentifierInfo
            {
                public S.Exp ObjectExp { get; }
                public TypeValue ObjectTypeValue { get; }
                public Name VarName { get; }
                public InstanceMember(S.Exp objectExp, TypeValue objectTypeValue, Name varName)
                {
                    ObjectExp = objectExp;
                    ObjectTypeValue = objectTypeValue;
                    VarName = varName;
                }
            }

            // f 
            public class Func : IdentifierInfo
            {
                public FuncValue FuncValue { get; }
                public Func(FuncValue funcValue)
                {
                    FuncValue = funcValue;
                }
            }

            // T
            public class Type : IdentifierInfo
            {
                public TypeValue.Normal TypeValue { get; }
                public Type(TypeValue.Normal typeValue)
                {
                    TypeValue = typeValue;
                }
            }

            // F => E.F
            public class EnumElem : IdentifierInfo
            {
                public TypeValue.Normal EnumTypeValue { get; }
                public EnumElemInfo ElemInfo { get; }

                public EnumElem(TypeValue.Normal enumTypeValue, EnumElemInfo elemInfo)
                {
                    EnumTypeValue = enumTypeValue;
                    ElemInfo = elemInfo;
                }
            }
        }
    }
}
