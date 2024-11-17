#include "NClassMemberFuncDecl.h"

#include <cassert>
#include <Infra/Exceptions.h>
#include "NClassDecl.h"

using namespace std;

namespace Citron {

NDecl* NClassMemberFuncDecl::GetNOuter()
{
    return _class.lock().get();
}

RDecl* NClassMemberFuncDecl::GetROuter()
{
    return _class.lock().get();
}

RIdentifier NClassMemberFuncDecl::GetIdentifier()
{
    return RIdentifier { name, typeParams.size(), NCommonFuncDeclComponent::GetParamIds() };
}

optional<RMember> NClassMemberFuncDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RMember> NClassMemberFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto sharedClass = _class.lock();
    assert(sharedClass);

    size_t baseTypeParamCount = sharedClass->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;

    return _class.lock()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

} // namespace Citron