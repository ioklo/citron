#include "REnumDecl.h"
#include "REnumElemDecl.h"
#include "RFactory.h"

using namespace std;

namespace Citron {

REnumDecl::REnumDecl(RTypeDeclOuter outer, TakeRef<RName> name, TakeRef<RFactoryPtr> rFactory)
    : outer{outer}, name{name.Take()}, rFactory{rFactory.Take()}
{
}

void REnumDecl::AddElem(REnumElemDecl* elem)
{
    elems.push_back(elem);
    elemsMap.emplace(elem->GetName(), elem);
}

RDecl* REnumDecl::GetOuter()
{
    return outer.GetDecl();
}

RIdentifier REnumDecl::GetIdentifier()
{
    return RIdentifier{name, genericsComp.GetTypeParamCount(), {}};
}

size_t REnumDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParamDecl* REnumDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeDecl* REnumDecl::GetTypeMember(InRef<RName> name, size_t typeParamCount)
{
    if (RTypeDecl* typeDecl = genericsComp.GetTypeMember(name, typeParamCount))
        return typeDecl;

    auto i = elemsMap.find(*name);
    if (i != elemsMap.end())
        return i->second;

    return nullptr;
}

optional<RDeclRes> REnumDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    // enumElem에서 검색, 같은 이름은 
    auto i = elemsMap.find(*name);
    if (i == elemsMap.end()) return nullopt;

    return RDeclRes_EnumElem(typeArgs, i->second);
}

optional<RDeclRes> REnumDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (auto o_member = genericsComp.ResolveTypeParam(name, explicitTypeParamsExceptOuterCount))
        return o_member;

    return nullopt;
}

RDecl* REnumDecl::RTypeDecl_GetDecl()
{
    return this;
}

RType* REnumDecl::GetOpenType()
{
    return rFactory->MakeEnumType(this, MakeOpenTypeArgs(*rFactory));
}

RDeclRes REnumDecl::ToRDeclRes(RTypeArguments* typeArgs)
{
    return RDeclRes_Enum(typeArgs, this);
}

void REnumDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron