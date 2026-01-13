#include "NStructCtorDecl.h"

#include <cassert>
#include "NStructDecl.h"

using namespace std;

namespace Citron
{

NStructCtorDecl::NStructCtorDecl(NStructDecl* _struct, RAccessor accessor, RStructCtorKind kind)
    : _struct{_struct}
    , accessor{accessor}
    , kind{kind}
    , NCommonFuncDeclComponent(RThisKind::Ptr, /*bSeqFunc*/false)
{
    NGenericsComponent::InitTypeParams({});
}

void NStructCtorDecl::InitFuncParameters(vector<RFuncParameter>&& parameters, bool bLastParameterVariadic)
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn_ForCtor(), move(parameters), bLastParameterVariadic);
}

NStructCtorDecl::~NStructCtorDecl() = default;

NDecl* NStructCtorDecl::GetNOuter()
{
    return _struct;
}

NFuncDeclOuter* NStructCtorDecl::GetNFuncDeclOuter()
{
    return _struct;
}

RDecl* NStructCtorDecl::GetROuter()
{
    return _struct;
}

RIdentifier NStructCtorDecl::GetIdentifier()
{
    return RIdentifier{ RName_Reserved("Ctor"), 0, NCommonFuncDeclComponent::GetParamIds()};
}

RTypeDecl* NStructCtorDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    return NGenericsComponent::GetTypeMember(name, typeParamCount);
}

RStructDecl* NStructCtorDecl::GetStructDecl()
{
    return _struct;
}

optional<Citron::RMember> NStructCtorDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RMember> NStructCtorDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{   
    if (auto o_member = NGenericsComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (auto o_member = NCommonFuncDeclComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _struct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

}

