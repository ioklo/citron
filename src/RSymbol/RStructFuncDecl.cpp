#include "RStructFuncDecl.h"
#include "RStructDecl.h"

using namespace std;

namespace Citron {

RStructFuncDecl::RStructFuncDecl(RStructDecl* _struct, RStructMemberAccessor accessor, TakeRef<RName> name, bool bSeqFunc)
    : _struct{_struct}, accessor{accessor}, name{name.Take()}
    , genericsComp{}
    , commonFuncDeclComp{bSeqFunc}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
}

void RStructFuncDecl::InitFuncReturnAndParams(bool bStatic, RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic)
{
    commonFuncDeclComp.InitFuncReturnAndParams(std::move(funcRet), bStatic ? (RThisKind)RThisKind_Static {} : RThisKind_Ref{_struct->GetOpenType()}, std::move(funcParameters), bLastParameterVariadic);
}

// from RDecl
RDecl* RStructFuncDecl::GetOuter()
{
    return _struct;
}

RIdentifier RStructFuncDecl::GetIdentifier()
{
    return RIdentifier{name, genericsComp.GetTypeParamCount(), commonFuncDeclComp.GetParamIds()};
}

size_t RStructFuncDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParamDecl* RStructFuncDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeDecl* RStructFuncDecl::GetTypeMember(InRef<RName> name, size_t typeParamCount)
{
    return genericsComp.GetTypeMember(name, typeParamCount);
}

optional<RDeclRes> RStructFuncDecl::GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RDeclRes> RStructFuncDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = genericsComp.ResolveIdentifierCore(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    if (auto o_member = commonFuncDeclComp.ResolveIdentifierCore(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _struct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}




} // namespace Citron