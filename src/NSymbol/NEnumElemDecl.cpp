#include "NEnumElemDecl.h"

#include "Infra/Exceptions.h"
#include "RSymbol/RFuncParameter.h"
#include "RSymbol/RFactory.h"
#include "NEnumDecl.h"

using namespace std;

namespace Citron {

NEnumElemDecl::NEnumElemDecl(NEnumDecl* _enum, const RName& name, const RFactoryPtr& rFactory)
    : _enum{_enum}
    , name{name}
    , rFactory{rFactory}
{
}

void NEnumElemDecl::AddVar(NEnumElemVarDecl* var)
{
    vars.push_back(var);
    varsMap.emplace(var->name, var);
}

NDecl* NEnumElemDecl::GetNOuter()
{
    return _enum;
}

RDeclRes NEnumElemDecl::ToRDeclRes(RTypeArguments* typeArgs)
{
    return RDeclRes_EnumElem(typeArgs, this);
}

RDecl* NEnumElemDecl::GetROuter()
{
    return _enum;
}

RIdentifier NEnumElemDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

RTypeDecl* NEnumElemDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    return nullptr;
}

optional<RDeclRes> NEnumElemDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    return GetVar(typeArgs, name);
}

optional<RDeclRes> NEnumElemDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

RType* NEnumElemDecl::GetOpenType()
{
    return rFactory->MakeEnumElemType(this, MakeOpenTypeArgs(*rFactory));
}

REnumDecl* NEnumElemDecl::GetBaseEnumDecl()
{
    return _enum;
}

optional<RDeclRes_EnumElemVar> NEnumElemDecl::GetVar(RTypeArguments* typeArgs, const RName& name)
{
    auto i = varsMap.find(name);
    if (i == varsMap.end()) return nullopt;

    return RDeclRes_EnumElemVar(typeArgs, i->second);
}

REnumElemVarDecl* NEnumElemDecl::GetVarDecl(size_t index)
{
    return vars[index];
}

size_t NEnumElemDecl::GetVarCount()
{
    return vars.size();
}

} // namespace Citron