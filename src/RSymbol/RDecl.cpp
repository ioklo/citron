#include "RDecl.h"

#include "Infra/Exceptions.h"
#include "Infra/Unreachable.h"

#include "RFactory.h"

using namespace std;

namespace Citron {

bool RDecl::IsDescendantOf(RDecl* container)
{
    auto* outer = GetOuter();
    if (outer == nullptr) return false;

    if (outer == container) return true;
    return outer->IsDescendantOf(container);
}

//bool RDecl::CanAccess(RDecl* target)
//{
//    auto accessModifier = target->GetAccessor();
//    auto* targetOuter = target->GetROuter();
//    if (targetOuter == nullptr)
//        return false;
//
//    switch (accessModifier)
//    {
//    case RAccessor::Public: return true;
//    case RAccessor::Protected: throw NotImplementedException();
//    case RAccessor::Private:
//    {
//        // 같은 경우는 허용
//        if (this == targetOuter)
//            return true;
//
//        // base클래스가 아니라 container에 속하는지를 본다
//        return IsDescendantOf(targetOuter);
//    }
//
//    default: unreachable();
//    }
//}

}