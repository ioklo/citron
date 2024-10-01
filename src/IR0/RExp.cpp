#include "RExp.h"

#include "RLoc.h"
#include "RTypeFactory.h"
#include "RStructMemberVarDecl.h"
#include "RClassMemberVarDecl.h"

namespace Citron {

RLocalRefExp::RLocalRefExp(const RLocPtr& innerLoc)
    : innerLoc(innerLoc)
{
}

RTypePtr RLocalRefExp::GetType(RTypeFactory& factory)
{
    auto innerLocType = innerLoc->GetType(factory);
    return factory.MakeLocalPtrType(std::move(innerLocType));
}

RClassMemberBoxRefExp::RClassMemberBoxRefExp(const RLocPtr& holder, const std::shared_ptr<RClassMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : holder(holder), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RClassMemberBoxRefExp::GetType(RTypeFactory& factory)
{
    auto declType = memberVarDecl->GetDeclType(*typeArgs, factory);
    return factory.MakeBoxPtrType(std::move(declType));
}

RStructIndirectMemberBoxRefExp::RStructIndirectMemberBoxRefExp(const RLocPtr& holder, const std::shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : holder(holder), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RStructIndirectMemberBoxRefExp::GetType(RTypeFactory& factory)
{
    auto declType = memberVarDecl->GetDeclType(*typeArgs, factory);
    return factory.MakeBoxPtrType(std::move(declType));
}

RStructMemberBoxRefExp::RStructMemberBoxRefExp(const RLocPtr& parent, const std::shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : parent(parent), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RStructMemberBoxRefExp::GetType(RTypeFactory& factory)
{
    auto declType = memberVarDecl->GetDeclType(*typeArgs, factory);

    return factory.MakeBoxPtrType(std::move(declType));
}

} // namespace Citron