#include "NClassFuncDecl.h"

#include <cassert>
#include "Infra/Exceptions.h"

#include "NClassDecl.h"

using namespace std;

namespace Citron {

NDecl* NClassFuncDecl::GetNOuter()
{
    return _class;
}

RDecl* NClassFuncDecl::GetROuter()
{
    return _class;
}

RIdentifier NClassFuncDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), NCommonFuncDeclComponent::GetParamIds() };
}

optional<RMember> NClassFuncDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RMember> NClassFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    size_t baseTypeParamCount = _class->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;

    return _class->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

} // namespace Citron