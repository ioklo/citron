using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.AbstractSyntax
{
    public enum VirtualModifier
    {
        Virtual,  // 시작
        Override, // 덮어씌우기
        New,      // Virtual의 끝, 테이블에 들어가지 않음
        Sealed,   // 더이상 오버라이드 불가능
    }
}
