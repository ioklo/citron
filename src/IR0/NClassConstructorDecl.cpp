#include "NClassConstructorDecl.h"

#include <cassert>
#include "NClassDecl.h"
using namespace std;

namespace Citron {

NDecl* NClassConstructorDecl::GetNOuter()
{
    return _class.lock().get();
}

RDecl* NClassConstructorDecl::GetROuter()
{
    return _class.lock().get();
}

RIdentifier NClassConstructorDecl::GetIdentifier()
{
    return RIdentifier { RName_Reserved("Constructor"), 0, NCommonFuncDeclComponent::GetParamIds() };
}

optional<RMember> NClassConstructorDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RMember> NClassConstructorDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto sharedClass = _class.lock();
    assert(sharedClass);

    auto baseTypeParamCount = sharedClass->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;


    return sharedClass->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

shared_ptr<RClassDecl> NClassConstructorDecl::GetClassDecl()
{
    return _class.lock();
}

} // namespace Citron