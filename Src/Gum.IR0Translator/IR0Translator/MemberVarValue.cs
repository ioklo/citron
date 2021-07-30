using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;

using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    // 분석 과정에서 사용하는 멤버 변수의 간접 참조, 타입 인자 정보를 포함한다
    class MemberVarValue : ItemValue
    {   
        ItemValueFactory factory;
        NormalTypeValue outer;
        IModuleMemberVarInfo info;

        public M.Name Name { get => info.GetName(); }
        public bool IsStatic { get => info.IsStatic(); }
        
        public MemberVarValue(ItemValueFactory factory, NormalTypeValue outer, IModuleMemberVarInfo info)
        {
            this.factory = factory;
            this.outer = outer;
            this.info = info;
        }

        internal override void FillTypeEnv(TypeEnvBuilder builder)
        {
            outer.FillTypeEnv(builder);
        }

        public TypeValue GetTypeValue()
        {
            var typeEnv = MakeTypeEnv();
            var type = info.GetDeclType();

            var typeValue = factory.MakeTypeValueByMType(type);
            return typeValue.Apply_TypeValue(typeEnv);
        }

        public R.Path.Nested GetRPath_Nested()
        {
            var rname = RItemFactory.MakeName(Name);

            return new R.Path.Nested(outer.GetRPath_Nested(), rname, R.ParamHash.None, default);
        }

        public sealed override R.Path GetRPath()
        {
            return GetRPath_Nested();
        }

        public override ItemValue Apply_ItemValue(TypeEnv typeEnv)
        {
            var appliedOuter = outer.Apply_NormalTypeValue(typeEnv);
            return factory.MakeMemberVarValue(appliedOuter, info);
        }
    }
}