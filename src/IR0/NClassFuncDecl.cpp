#include "NClassFuncDecl.h"

#include <cassert>
#include <Infra/Exceptions.h>
#include "NClassDecl.h"

using namespace std;

namespace Citron {

NDecl* NClassFuncDecl::GetNOuter()
{
    return _class.lock().get();
}

RDecl* NClassFuncDecl::GetROuter()
{
    return _class.lock().get();
}

RIdentifier NClassFuncDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), NCommonFuncDeclComponent::GetParamIds() };
}

optional<RMember> NClassFuncDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RMember> NClassFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto sharedClass = _class.lock();
    assert(sharedClass);

    size_t baseTypeParamCount = sharedClass->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;

    return _class.lock()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

} // namespace Citron