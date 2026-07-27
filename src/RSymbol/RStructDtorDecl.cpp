#include "RStructDtorDecl.h"
#include "RStructDecl.h"

using namespace std;

namespace Citron {

RStructDtorDecl::RStructDtorDecl(RDeclKey&& key, RStructDecl* _struct, RStructMemberAccessor accessor)
    : key{std::move(key)}, _struct{_struct}, accessor{accessor}, commonFuncDeclComp{/*bSeqFunc*/false}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
}

RDeclKey& RStructDtorDecl::GetDeclKey()
{
    return key;
}

// from RDecl
RDecl* RStructDtorDecl::GetOuter()
{
    return this;
}

RName* RStructDtorDecl::TryGetName()
{
    return nullptr;
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