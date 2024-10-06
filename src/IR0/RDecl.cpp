#include "RDecl.h"
#include "RTypeFactory.h"

namespace Citron {

std::string RDecl::GetModuleName()
{
    return GetOuter()->GetModuleName();
}

RDeclIdPtr RDecl::GetDeclId(RTypeFactory& factory)
{
    auto outer = GetOuter();
    if (outer)
        return factory.MakeChildDeclId(outer->GetDeclId(factory), GetIdentifier());
    else
        return factory.MakeDeclId(GetModuleName(), GetIdentifier());
}

}