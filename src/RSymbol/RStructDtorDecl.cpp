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
    return RIdentifier{RName_Reserved{RName_ReservedName::Dtor}, {}};
}

size_t RStructDtorDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParam* RStructDtorDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeParam* RStructDtorDecl::GetTypeParam(InRef<RName> name)
{
    return nullptr;
}

RTypeDecl* RStructDtorDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RStructDtorDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron