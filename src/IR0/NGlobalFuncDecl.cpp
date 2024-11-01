#include "NGlobalFuncDecl.h"

using namespace std;

namespace Citron {

optional<RMember> NGlobalFuncDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    // 람다는 검색시키지 않는다
    // 현재 함수에서 Declaration을 할 수 없기 때문에 
    return nullopt;
}

}