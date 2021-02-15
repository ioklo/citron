using M = Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    // 분석 과정에서 사용하는 멤버 변수의 간접 참조, 타입 인자 정보를 포함한다
    class MemberVarValue : ItemValue
    {
        // 어느 타입에 속해 있는지
        // NormalTypeValue outer;
        // M.MemberVarInfo info;
        // NormalTypeValue typeValue;

        TypeValueFactory factory;
        TypeValue outer;
        M.MemberVarInfo info;

        public M.Name Name { get => info.Name; }
        public bool IsStatic { get => info.IsStatic; }
        
        public MemberVarValue(TypeValueFactory factory, TypeValue outer, M.MemberVarInfo info)
        {
            this.factory = factory;
            this.outer = outer;
            this.info = info;
        }

        internal override int FillTypeEnv(TypeEnvBuilder builder)
        {
            return outer.FillTypeEnv(builder) + 1;
        }

        public TypeValue GetTypeValue()
        {
            var typeEnv = MakeTypeEnv();
            var typeValue = factory.MakeTypeValue(info.Type);
            return typeValue.Apply(typeEnv);
        }
    }
}
