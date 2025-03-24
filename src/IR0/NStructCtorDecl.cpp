#include "NStructCtorDecl.h"

#include <cassert>
#include "NStructDecl.h"

using namespace std;

namespace Citron
{

NStructCtorDecl::NStructCtorDecl(weak_ptr<NStructDecl> _struct, RAccessor accessor, bool bTrivial)
    : NCommonFuncDeclComponent(/*bStatic*/ false, /*bSeqFunc*/ false, /*typeParams*/ {})
    , _struct(std::move(_struct))
    , accessor(accessor)
    , bTrivial(bTrivial)
{
}

void NStructCtorDecl::InitFuncParameters(std::vector<RFuncParameter> parameters, bool bLastParameterVariadic)
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn_ForCtor(), std::move(parameters), bLastParameterVariadic);
}

NStructCtorDecl::~NStructCtorDecl() = default;

NDecl* NStructCtorDecl::GetNOuter()
{
    return _struct.lock().get();
}

RDecl* NStructCtorDecl::GetROuter()
{
    return _struct.lock().get();
}

RIdentifier NStructCtorDecl::GetIdentifier()
{
    return RIdentifier { RName_Reserved("Ctor"), 0, NCommonFuncDeclComponent::GetParamIds() };
}

shared_ptr<RStructDecl> NStructCtorDecl::GetStructDecl()
{
    return _struct.lock();
}

optional<Citron::RMember> NStructCtorDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RMember> NStructCtorDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto sharedStruct = _struct.lock();
    assert(sharedStruct);

    auto baseTypeParamCount = sharedStruct->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;

    return sharedStruct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

}

