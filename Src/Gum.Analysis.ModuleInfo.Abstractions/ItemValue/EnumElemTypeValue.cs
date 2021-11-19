using Gum.Collections;
using System.Diagnostics;

using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;

namespace Gum.Analysis
{
    // S.First, S.Second(int i, short s)
    [AutoConstructor]
    public partial class EnumElemTypeValue : NormalTypeValue
    {
        RItemFactory ritemFactory;
        ItemValueFactory itemValueFactory;
        public EnumTypeValue Outer { get; }
        IModuleEnumElemInfo elemInfo;        
        
        public bool IsStandalone()
        {
            return elemInfo.IsStandalone();
        }

        public override NormalTypeValue Apply_NormalTypeValue(TypeEnv typeEnv)
        {
            var appliedOuter = Outer.Apply_EnumTypeValue(typeEnv);
            return itemValueFactory.MakeEnumElemTypeValue(appliedOuter, elemInfo);
        }

        public override R.Path.Nested GetRPath_Nested()
        {
            var router = Outer.GetRPath_Nested();
            var rname = RItemFactory.MakeName(elemInfo.GetName());
            Debug.Assert(router != null);

            return new R.Path.Nested(router, rname, R.ParamHash.None, default);
        }

        public ImmutableArray<ParamInfo> GetConstructorParamTypes()
        {
            var fieldInfos = elemInfo.GetFieldInfos();

            var builder = ImmutableArray.CreateBuilder<ParamInfo>(fieldInfos.Length);
            foreach(var field in fieldInfos)
            {
                var fieldType = itemValueFactory.MakeTypeValueByMType(field.GetDeclType());
                var appliedFieldType = fieldType.Apply_TypeValue(Outer.MakeTypeEnv());

                builder.Add(new ParamInfo(R.ParamKind.Normal, appliedFieldType)); // TODO: EnumElemFields에 ref를 지원할지
            }

            return builder.MoveToImmutable();
        }

        public override ItemQueryResult GetMember(M.Name memberName, int typeParamCount)
        {
            foreach (var field in elemInfo.GetFieldInfos())
            {
                if (field.GetName().Equals(memberName))
                {
                    if (typeParamCount != 0)
                        return ItemQueryResult.Error.VarWithTypeArg.Instance;

                    return new ItemQueryResult.MemberVar(this, field);
                }
            }

            return ItemQueryResult.NotFound.Instance;
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => new R.EnumElemMemberLoc(instance, member);

        internal override void FillTypeEnv(TypeEnvBuilder builder)
        {
            Outer.FillTypeEnv(builder);
        }

        public override int GetTotalTypeParamCount()
        {
            return Outer.GetTotalTypeParamCount(); // Elem자체는 typeParams을 가질 수 없다
        }
    }
}
