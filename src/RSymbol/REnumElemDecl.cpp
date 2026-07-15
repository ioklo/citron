#include "REnumElemDecl.h"
#include "Infra/Exceptions.h"
#include "REnumDecl.h"
#include "REnumElemVarDecl.h"
#include "RFactory.h"

using namespace std;

namespace Citron {

REnumElemDecl::REnumElemDecl(REnumDecl* _enum, TakeRef<RName> name, TakeRef<RFactoryPtr> rFactory)
    : _enum{_enum}, name{name.Take()}, rFactory{rFactory.Take()}
{
}

void REnumElemDecl::AddVar(REnumElemVarDecl* var)
{
    vars.push_back(var);
    varsMap.emplace(var->GetName(), var);
}

optional<RDeclRes_EnumElemVar> REnumElemDecl::ResolveVar(RTypeArguments* typeArgs, InRef<RName> name)
{
    auto i = varsMap.find(*name);
    if (i == varsMap.end()) return nullopt;

    return RDeclRes_EnumElemVar(typeArgs, i->second);
}

RDecl* REnumElemDecl::GetOuter()
{
    return _enum;
}

RIdentifier REnumElemDecl::GetIdentifier()
{
    return RIdentifier{name, {}};
}

size_t REnumElemDecl::GetTypeParamCount()
{
    return 0;
}

RTypeParamDecl* REnumElemDecl::GetTypeParam(size_t index)
{
    return nullptr;
}

RTypeDecl* REnumElemDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RDeclRes> REnumElemDecl::ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    return ResolveVar(typeArgs, name);
}

optional<RDeclRes> REnumElemDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    return optional<RDeclRes>();
}

RDecl* REnumElemDecl::RTypeDecl_GetDecl()
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

RType* REnumElemDecl::GetOpenType()
{
    return rFactory->MakeEnumElemType(this, MakeOpenTypeArgs(*rFactory));
}

RDeclRes REnumElemDecl::ToRDeclRes(RTypeArguments* typeArgs)
{
    return RDeclRes_EnumElem(typeArgs, this);
}

void REnumElemDecl::Accept(RTypeDeclVisitor& visitor)
{
    visitor.Visit(this);
}

} // namespace Citron