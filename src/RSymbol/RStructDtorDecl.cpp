#include "RStructDtorDecl.h"
#include "RStructDecl.h"

using namespace std;

namespace Citron {

RStructDtorDecl::RStructDtorDecl(RStructDecl* _struct, RStructMemberAccessor accessor)
    : _struct{_struct}, accessor{accessor}, commonFuncDeclComp{/*bSeqFunc*/false}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
}

// from RDecl
RDecl* RStructDtorDecl::GetOuter()
{
    return this;
}

RIdentifier RStructDtorDecl::GetIdentifier()
{
    return RIdentifier{RName_Reserved{RName_ReservedName::Dtor}, 0, {}};
}

size_t RStructDtorDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParamDecl* RStructDtorDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeDecl* RStructDtorDecl::GetTypeMember(InRef<RName> name, size_t typeParamCount)
{
    return nullptr;
}

optional<RDeclRes> RStructDtorDecl::GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<RDeclRes> RStructDtorDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = commonFuncDeclComp.ResolveIdentifierCore(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return _struct->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

} // namespace Citron