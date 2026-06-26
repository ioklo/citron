#include "NStructFuncDecl.h"
#include <cassert>
#include "NStructDecl.h"

using namespace std;

namespace Citron
{

NStructFuncDecl::NStructFuncDecl(
    NStructDecl* _struct, RAccessor accessor, bool bStatic, bool bSeqFunc,
    const string& name)
    : _struct{_struct}
    , accessor{accessor}
    , name{name}
    , _static{bStatic}
    , NFuncDeclImpl_UsingNCommonFuncDeclComponent<RStructFuncDecl>{bSeqFunc}
{   
}

void NStructFuncDecl::InitFuncReturnAndParams(RFuncReturn&& funcRet, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic)
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(move(funcRet), _static ? (RThisKind)RThisKind_Static {} : RThisKind_Ref{_struct->GetOpenType()}, move(funcParameters), bLastParameterVariadic);
}

NDecl* NStructFuncDecl::GetNOuter()
{
    return _struct;
}

RDecl* NStructFuncDecl::GetROuter()
{
    return _struct;
}

RIdentifier NStructFuncDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), NGenericsComponent::GetTypeParamCount(), NCommonFuncDeclComponent::GetParamIds() };
}

RTypeDecl* NStructFuncDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    return NGenericsComponent::GetTypeMember(name, typeParamCount);
}

optional<RDeclRes> NStructFuncDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RDeclRes> NStructFuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{   
    if (auto o_member = NGenericsComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (auto o_member = NCommonFuncDeclComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _struct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

}