#include "NStructDtorDecl.h"
#include "NStructDecl.h"

using namespace std;

namespace Citron {

NStructDtorDecl::NStructDtorDecl(RAccessor accessor, NStructDecl* _struct)
    : accessor{accessor}, _struct{_struct}
    , NFuncDeclImpl_UsingNCommonFuncDeclComponent<RStructDtorDecl>{/*bSeqFunc*/false}
{
    NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn_None{}, RThisKind_Ref{_struct->GetOpenType()}, /*funcParameters*/{}, /*bLastParamVariadic*/false);
}

NDecl* NStructDtorDecl::GetNOuter()
{
    return _struct;
}

RDecl* NStructDtorDecl::GetROuter()
{
    return _struct;
}

RTypeDecl* NStructDtorDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    return nullptr;
}

optional<RDeclRes> NStructDtorDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RDeclRes> NStructDtorDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{   
    if (auto o_member = NCommonFuncDeclComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _struct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

} // namespace Citron