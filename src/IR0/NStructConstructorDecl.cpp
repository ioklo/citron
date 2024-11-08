#include "NStructConstructorDecl.h"

#include <cassert>
#include "NStructDecl.h"

using namespace std;

namespace Citron
{

NStructConstructorDecl::NStructConstructorDecl(weak_ptr<NStructDecl> _struct, RAccessor accessor, bool bTrivial)
    : NCommonFuncDeclComponent(/*typeParams*/ {})
    , _struct(std::move(_struct))
    , accessor(accessor)
    , bTrivial(bTrivial)
    
{
}

void NStructConstructorDecl::InitFuncParameters(std::vector<RFuncParameter> parameters, bool bLastParameterVariadic)
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn_ForConstructor(), std::move(parameters), bLastParameterVariadic);
}

NStructConstructorDecl::~NStructConstructorDecl() = default;

NDecl* NStructConstructorDecl::GetNOuter()
{
    return _struct.lock().get();
}

RDecl* NStructConstructorDecl::GetROuter()
{
    return _struct.lock().get();
}

RIdentifier NStructConstructorDecl::GetIdentifier()
{
    return RIdentifier { RName_Reserved("Constructor"), 0, NCommonFuncDeclComponent::GetParamIds() };
}

shared_ptr<RStructDecl> NStructConstructorDecl::GetStructDecl()
{
    return _struct.lock();
}

optional<Citron::RMember> NStructConstructorDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RMember> NStructConstructorDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto sharedStruct = _struct.lock();
    assert(sharedStruct);

    auto baseTypeParamCount = sharedStruct->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;

    return sharedStruct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

}

