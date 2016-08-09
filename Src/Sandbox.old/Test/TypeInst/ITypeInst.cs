using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Test.Type
{
    // ObjectTypeVar, FuncTypeVar, TypeWithVar    
    // 타입 컨텍스트 안에서 의미를 찾을 수 있는 타입입니다.
    // 1. 클래스나 함수에 선언에 타입 인자로 넘어간 타입 심볼 T, F
    // 2. 타입 인자가 필요한 클래스에 타입인자로 바인딩한 타입 SomeType<T>, SomeType<int> ...
    interface IType
    {
        // TypeDef 
        IEnumerable<IType> GetMemberTypes(Environment env, string memberName);        

        // FuncDef
    }
}
