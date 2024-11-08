#include "RDecl.h"

#include <Infra/Exceptions.h>
#include <Infra/Unreachable.h>

#include "RTypeFactory.h"

using namespace std;

namespace Citron {

bool RDecl::IsDescendantOf(RDecl* container)
{
    auto* outer = GetROuter();
    if (outer == nullptr) return false;

    if (outer == container) return true;
    return outer->IsDescendantOf(container);
}

bool RDecl::CanAccess(RDecl* target)
{
    auto accessModifier = target->GetAccessor();
    auto* targetOuter = target->GetROuter();
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

    default: unreachable();
    }
}

size_t RDecl::GetTypeParamCount()
{
    return GetIdentifier().typeParamCount;
}

size_t RDecl::GetAllTypeParamCount()
{
    auto* outer = GetROuter();
    if (!outer) return GetTypeParamCount();

    return outer->GetAllTypeParamCount() + GetTypeParamCount();
}

RTypeArgumentsPtr RDecl::MakeOpenTypeArgs(RTypeFactory& factory)
{
    // gather reversely
    auto allTypeParamCount = GetAllTypeParamCount();    
    
    vector<RTypePtr> items;
    items.reserve(allTypeParamCount);
    for(int i = 0; i < allTypeParamCount; i++)
    {
        auto typeVar = factory.MakeTypeVarType(i);
        items.push_back(typeVar);
    }

    return factory.MakeTypeArguments(items);
}

std::string RDecl::GetModuleName()
{
    return GetROuter()->GetModuleName();
}


}