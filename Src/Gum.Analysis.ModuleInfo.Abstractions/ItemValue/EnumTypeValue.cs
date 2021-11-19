using System;
using Gum.Collections;

using M = Gum.CompileTime;
using R = Gum.IR0;
using Pretune;

namespace Gum.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    public partial class EnumTypeValue : NormalTypeValue
    {
        ItemValueFactory itemValueFactory;
        ItemValueOuter outer;
        IModuleEnumInfo enumInfo;
        ImmutableArray<TypeValue> typeArgs;        

        public override R.Path.Nested GetRPath_Nested()
        {
            var rname = RItemFactory.MakeName(enumInfo.GetName());
            var rtypeArgs = RItemFactory.MakeRTypes(typeArgs);

            return new R.Path.Nested(outer.GetRPath(), rname, new R.ParamHash(enumInfo.GetTypeParams().Length, default), rtypeArgs);
        }

        public EnumTypeValue Apply_EnumTypeValue(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply_TypeValue(typeEnv));
            return itemValueFactory.MakeEnumTypeValue(appliedOuter, enumInfo, appliedTypeArgs);
        }

        public sealed override NormalTypeValue Apply_NormalTypeValue(TypeEnv typeEnv)
        {
            return Apply_EnumTypeValue(typeEnv);
        }

        //
        public override ItemQueryResult GetMember(M.Name memberName, int typeParamCount) 
        {
            // shortcut
            if (typeParamCount != 0)
                return ItemQueryResult.NotFound.Instance;

            var elemInfo = enumInfo.GetElem(memberName);
            if (elemInfo == null) return ItemQueryResult.NotFound.Instance;

            return new ItemQueryResult.EnumElem(this, elemInfo);
        }

        public EnumElemTypeValue? GetElement(string name)
        {
            var elemInfo = enumInfo.GetElem(new M.Name.Normal(name));
            if (elemInfo == null) return null;

            return itemValueFactory.MakeEnumElemTypeValue(this, elemInfo);
        }

        public override TypeValue? GetMemberType(M.Name memberName, ImmutableArray<TypeValue> typeArgs) 
        {
            // shortcut
            if (typeArgs.Length != 0)
                return null;

            var elemInfo = enumInfo.GetElem(memberName);
            if (elemInfo == null) return null;

            return itemValueFactory.MakeEnumElemTypeValue(this, elemInfo);
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested member)
            => throw new InvalidOperationException();

        internal override void FillTypeEnv(TypeEnvBuilder builder)
        {
            if (outer != null)
                outer.FillTypeEnv(builder);

            for (int i = 0; i < enumInfo.GetTypeParams().Length; i++)
                builder.Add(typeArgs[i]);
        }

        public override int GetTotalTypeParamCount()
        {
            return outer.GetTotalTypeParamCount() + enumInfo.GetTypeParams().Length;
        }
    }
}
