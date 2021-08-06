using System;
using R = Gum.IR0;
using Pretune;
using Gum.Collections;

namespace Gum.IR0Translator
{
    [AutoConstructor, ImplementIEquatable]
    partial class ClassTypeValue : NormalTypeValue
    {
        ItemValueFactory itemValueFactory;

        ItemValueOuter outer;
        IModuleClassInfo classInfo;
        ImmutableArray<TypeValue> typeArgs;

        public ClassTypeValue? GetBaseType() { throw new NotImplementedException(); }

        // except itself
        public bool IsBaseOf(ClassTypeValue derivedType)
        {
            ClassTypeValue? curBaseType = derivedType.GetBaseType();

            while(curBaseType != null)
            {
                if (Equals(curBaseType)) return true;
                curBaseType = curBaseType.GetBaseType();
            }

            return false;
        }

        public override NormalTypeValue Apply_NormalTypeValue(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply(typeEnv);
            var appliedTypeArgs = ImmutableArray.CreateRange(typeArgs, typeArg => typeArg.Apply_TypeValue(typeEnv));
            return itemValueFactory.MakeTypeValue(appliedOuter, classInfo, appliedTypeArgs);
        }

        public override R.Path.Nested GetRPath_Nested()
        {
            var rname = RItemFactory.MakeName(classInfo.GetName());
            var rtypeArgs = RItemFactory.MakeRTypes(typeArgs);

            return new R.Path.Nested(outer.GetRPath(), rname, new R.ParamHash(classInfo.GetTypeParams().Length, default), rtypeArgs);
        }

        public override R.Loc MakeMemberLoc(R.Loc instance, R.Path.Nested memberPath)
        {
            return new R.ClassMemberLoc(instance, memberPath);
        }

        public override int GetTotalTypeParamCount()
        {
            return outer.GetTotalTypeParamCount() + classInfo.GetTypeParams().Length;
        }

        internal override void FillTypeEnv(TypeEnvBuilder builder)
        {
            if (outer != null)
                outer.FillTypeEnv(builder);

            for (int i = 0; i < classInfo.GetTypeParams().Length; i++)
                builder.Add(typeArgs[i]);
        }

        public IModuleConstructorInfo? GetAutoConstructor()
        {
            return classInfo.GetAutoConstructor();
        }
    }
}
