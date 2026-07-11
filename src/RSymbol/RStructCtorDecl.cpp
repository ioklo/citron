#include "RStructCtorDecl.h"
#include "RStructDecl.h"

using namespace std;

namespace Citron {

RStructCtorDecl::RStructCtorDecl(RStructDecl* _struct, RStructMemberAccessor accessor, RStructCtorKind kind)
    : _struct{_struct}, accessor{accessor}, kind{kind}
    , genericsComp{}
    , commonFuncDeclComp{/*bSeqFunc*/false}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
}

void RStructCtorDecl::InitFuncParameters(std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    commonFuncDeclComp.InitFuncReturnAndParams(RFuncReturn_None{}, RThisKind_Static{}, std::move(funcParameters), bLastParameterVariadic);
}

// from RDecl
RDecl* RStructCtorDecl::GetOuter()
{
    return this;
}

RIdentifier RStructCtorDecl::GetIdentifier()
{
    return RIdentifier{RName_Reserved{RName_ReservedName::Ctor}, 0, commonFuncDeclComp.GetParamIds()};
}

size_t RStructCtorDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParamDecl* RStructCtorDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeDecl* RStructCtorDecl::GetTypeMember(InRef<RName> name, size_t typeParamCount)
{
    return genericsComp.GetTypeMember(name, typeParamCount);
}

optional<RDeclRes> RStructCtorDecl::GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RDeclRes> RStructCtorDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = genericsComp.ResolveIdentifierCore(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (auto o_member = commonFuncDeclComp.ResolveIdentifierCore(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _struct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

} // namespace Citron