#include "RStructMemberVarDecl.h"

namespace Citron
{

RStructMemberVarDecl::RStructMemberVarDecl(std::weak_ptr<RStructDecl> _struct, RAccessor accessor, bool bStatic, std::string name)
    : _struct(std::move(_struct))
    , accessor(accessor)
    , bStatic(bStatic)
    , name(std::move(name))
{
}

void RStructMemberVarDecl::InitDeclType(RTypePtr declType)
{
    this->declType = std::move(declType);
}

}