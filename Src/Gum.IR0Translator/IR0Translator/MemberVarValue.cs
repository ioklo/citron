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
        // 어느 타입에 속해 있는지
        // NormalTypeValue outer;
        // M.MemberVarInfo info;
        // NormalTypeValue typeValue;

        ItemValueFactory factory;
        TypeValue outer;
        M.MemberVarInfo info;

        public M.Name Name { get => info.Name; }
        public bool IsStatic { get => info.IsStatic; }
        
        public MemberVarValue(ItemValueFactory factory, TypeValue outer, M.MemberVarInfo info)
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
            var typeValue = factory.MakeTypeValue(info.Type);
            return typeValue.Apply_TypeValue(typeEnv);
        }

        public override R.Path GetRType()
        {
            throw new NotImplementedException();
        }
    }
}
