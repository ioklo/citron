#include "RNode.h"
#include "RAccessor.h"

using namespace std;

namespace Citron {

bool RNode::IsDescendantOf(RNode* node)
{
    if (outer == nullptr) return false;
    if (outer == node) return true;
    return outer->IsDescendantOf(node);
}

bool RNode::CanAccess(RNode* target)
{
    auto accessModifier = target->GetAccessor();
    auto* targetOuter = target->outer;
    if (targetOuter == nullptr)
        return false;

    switch (accessModifier)
    {
    case RAccessor::Public: return true;
    case RAccessor::Protected: throw NotImplementedException();
    case RAccessor::Private:
    {
        // 같은 경우는 허용
        if (this == targetOuter)
            return true;

        // base클래스가 아니라 container에 속하는지를 본다
        return IsDescendantOf(targetOuter);
    }
    }
}

} // namespace Citron