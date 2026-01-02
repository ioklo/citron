#include "NStructDtorDecl.h"
#include "NStructDecl.h"

using namespace std;

namespace Citron {

NStructDtorDecl::NStructDtorDecl(RAccessor accessor, NStructDecl* _struct)
    : accessor{accessor}, _struct{_struct}
    , NCommonFuncDeclComponent{/*bStatic*/false, /*bSeqFunc*/false}
{
    NCommonFuncDeclComponent::InitTypeParams({});
    NCommonFuncDeclComponent::InitFuncReturnAndParams(RFuncReturn_ForCtor{}, {}, /*bLastParamVariadic*/false);
}

NDecl* NStructDtorDecl::GetNOuter()
{
    return _struct;
}

NFuncDeclOuter* NStructDtorDecl::GetNFuncDeclOuter()
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

optional<RMember> NStructDtorDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RMember> NStructDtorDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{   
    if (auto o_member = NCommonFuncDeclComponent::ResolveIdentifier(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _struct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

} // namespace Citron