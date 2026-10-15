#include "RTypeAliasDecl.h"
#include "RMember.h"

using namespace std;

namespace Citron {

RTypeAliasDecl::RTypeAliasDecl(RDeclKey&& key, RTypeDeclOuter&& outer, RName&& name)
    : key{move(key)}, outer{move(outer)}, name{move(name)}
{
}

void RTypeAliasDecl::Init(vector<RTypeParam*>&& typeParams, RTypeAliasTarget&& target)
{   
    genericsComponent.InitTypeParams(move(typeParams));
    this->target = move(target);
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
    return genericsComponent.GetTypeParamCount();
}

RTypeParam* RTypeAliasDecl::GetTypeParam(size_t index)
{
    return genericsComponent.GetTypeParam(index);
}

RTypeParam* RTypeAliasDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComponent.GetTypeParam(name);
}

RTypeDecl* RTypeAliasDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RTypeAliasDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron