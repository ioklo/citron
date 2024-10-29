#include "NDecl.h"
#include "RTypeFactory.h"

#include <Infra/Exceptions.h>
#include <Infra/Unreachable.h>
#include "RAccessor.h"

namespace Citron {

std::string NDecl::GetModuleName()
{
    return GetOuter()->GetModuleName();
}

bool NDecl::IsDescendantOf(NDecl* container)
{
    auto* outer = GetOuter();
    if (outer == nullptr) return false;

    if (outer == container) return true;
    return outer->IsDescendantOf(container);
}

bool NDecl::CanAccess(NDecl* target)
{
    auto accessModifier = target->GetAccessor();
    auto* targetOuter = target->GetOuter();
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

RDeclIdPtr NDecl::GetDeclId(RTypeFactory& factory)
{
    auto outer = GetOuter();
    if (outer)
        return factory.MakeChildDeclId(outer->GetDeclId(factory), GetIdentifier());
    else
        return factory.MakeDeclId(GetModuleName(), GetIdentifier());
}

}