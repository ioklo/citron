#include "NStructMemberFuncDecl.h"
#include <cassert>
#include "NStructDecl.h"

using namespace std;

namespace Citron
{

NStructMemberFuncDecl::NStructMemberFuncDecl(std::weak_ptr<NStructDecl> _struct, RAccessor accessor, std::string name, std::vector<std::string>&& typeParams, bool bStatic)
    : NCommonFuncDeclComponent(std::move(typeParams))
    , _struct(std::move(_struct))
    , accessor(accessor)
    , name(std::move(name))
    , typeParams(std::move(typeParams))
    , bStatic(bStatic)
{
}

void NStructMemberFuncDecl::InitFuncReturnAndParams(RTypePtr funcReturn, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic)
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn_Set(std::move(funcReturn)), std::move(funcParameters), bLastParameterVariadic);
}

NDecl* NStructMemberFuncDecl::GetNOuter()
{
    return _struct.lock().get();
}

RDecl* NStructMemberFuncDecl::GetROuter()
{
    return _struct.lock().get();
}

RIdentifier NStructMemberFuncDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), typeParams.size(), NCommonFuncDeclComponent::GetParamIds() };
}

optional<RMember> NStructMemberFuncDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RMember> NStructMemberFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto sharedStruct = _struct.lock();
    assert(sharedStruct);

    size_t baseTypeParamCount = sharedStruct->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;

    return sharedStruct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

}