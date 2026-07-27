#include "REnumDecl.h"
#include "REnumElemDecl.h"
#include "RFactory.h"
#include "RTypeRes.h"
#include "RDeclRes.h"
#include "RMember.h"

using namespace std;

namespace Citron {

REnumDecl::REnumDecl(RDeclKey&& key, RTypeDeclOuter outer, RName&& name, TakeRef<RFactoryPtr> rFactory)
    : key{std::move(key)}, outer{outer}, name{std::move(name)}, rFactory{rFactory.Take()}
{
}

void REnumDecl::AddElem(REnumElemDecl* elem)
{
    elems.push_back(elem);
    elemsMap.emplace(elem->GetName(), elem);
}

RDeclKey& REnumDecl::GetDeclKey()
{
    return key;
}

RDecl* REnumDecl::GetOuter()
{
    return outer.GetDecl();
}

RName* REnumDecl::TryGetName()
{
    return &name;
}

size_t REnumDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* REnumDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* REnumDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* REnumDecl::GetTypeMember(InRef<RName> name)
{
    // enumElem에서 검색, 같은 이름은 
    auto i = elemsMap.find(*name);
    if (i != elemsMap.end())
        return i->second;

    return nullptr;
}

optional<RMember> REnumDecl::GetMember(InRef<RName> name)
{
    auto i = elemsMap.find(*name);
    if (i != elemsMap.end())
        return RMember_EnumElem{i->second};

    return nullopt;
}

RDecl* REnumDecl::RTypeDecl_GetDecl()
{
    return this;
}

void REnumDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron