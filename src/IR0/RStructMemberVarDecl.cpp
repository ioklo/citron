#include "RStructMemberVarDecl.h"
#include <cassert>
#include "RStructDecl.h"
#include "RTypeFactory.h"

namespace Citron {

RStructMemberVarDecl::RStructMemberVarDecl(std::weak_ptr<RStructDecl> _struct, RAccessor accessor, bool bStatic, std::string name)
    : _struct(std::move(_struct))
    , accessor(accessor)
    , bStatic(bStatic)
    , name(std::move(name))
{
}

void RStructMemberVarDecl::InitDeclType(const RTypePtr& declType)
{
    this->declType = declType;
}

RTypePtr RStructMemberVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    assert(declType != nullptr);
    return declType->Apply(typeArgs, factory);
}

RTypePtr RStructMemberVarDecl::GetStructType(const RTypeArgumentsPtr& typeArgs, RTypeFactory& factory)
{   
    return factory.MakeStructType(_struct.lock(), typeArgs);
}

RDecl* RStructMemberVarDecl::GetOuter()
{
    return _struct.lock().get();
}

RIdentifier RStructMemberVarDecl::GetIdentifier()
{
    return RIdentifier { RNormalName(name), 0, {} };
}

} // namespace Citron