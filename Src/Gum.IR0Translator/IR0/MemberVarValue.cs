using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    // 분석 과정에서 사용하는 멤버 변수의 간접 참조, 타입 인자 정보를 포함한다
    public class MemberVarValue
    {
        // 어느 타입에 속해 있는지
        public TypeValue.Normal Outer { get; }
        public Name Name { get => info.Name; }
        public bool IsStatic { get => info.IsStatic; }
        MemberVarInfo info;
        
        public MemberVarValue(TypeValue.Normal outer, MemberVarInfo info)
        {
            Outer = outer;
            this.info = info;
        }
    }
}
