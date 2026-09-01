#include "RTypeAliasDecl.h"

#include <cassert>

#include "RMember.h"

using namespace std;

namespace Citron {

RTypeAliasDecl::RTypeAliasDecl(RDeclKey&& key, RTypeDeclOuter outer, RName&& name)
    : key{move(key)}, outer{outer}, name{move(name)}
{
}

void RTypeAliasDecl::InitTargetType(RType* targetType)
{
    assert(!this->targetType);
    assert(targetType);
    this->targetType = targetType;
}

RType* RTypeAliasDecl::GetTargetType()
{
    assert(targetType);
    return targetType;
}

RDeclKey& RTypeAliasDecl::GetDeclKey()
{
    return key;
}

RDecl* RTypeAliasDecl::GetOuter()
{
    return outer.GetDecl();
}

RName* RTypeAliasDecl::TryGetName()
{
    return &name;
}

size_t RTypeAliasDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParam* RTypeAliasDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeParam* RTypeAliasDecl::GetTypeParam(InRef<RName> name)
{
    return nullptr;
}

RTypeDecl* RTypeAliasDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RTypeAliasDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

RDecl* RTypeAliasDecl::RTypeDecl_GetDecl()
{
    return this;
}

void RTypeAliasDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron
