#include "NStructDtorDecl.h"
#include "NStructDecl.h"

using namespace std;

namespace Citron {

NStructDtorDecl::NStructDtorDecl(RAccessor accessor, NStructDecl* _struct)
    : accessor{accessor}, _struct{_struct}, NCommonFuncDeclComponent{false, false, {}}
{
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

std::optional<RMember> NStructDtorDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory)
{
    auto baseTypeParamCount = _struct->GetAllTypeParamCount();
    if (auto oMember = NCommonFuncDeclComponent::ResolveIdentifier(baseTypeParamCount, name, explicitTypeParamsExceptOuterCount, factory))
        return oMember;

    return _struct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

} // namespace Citron