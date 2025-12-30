#include "NEnumElemVarDecl.h"

#include "Infra/Exceptions.h"

#include "RSymbol/RTypes.h"
#include "NEnumDecl.h"

using namespace std;

namespace Citron {

NEnumElemVarDecl::NEnumElemVarDecl(NEnumElemDecl* enumElem, const RName& name)
    : enumElem{enumElem}
    , name{name}
{
}

void NEnumElemVarDecl::Init(RType* declType)
{
    this->declType = declType;
}

NDecl* NEnumElemVarDecl::GetNOuter()
{
    return enumElem;
}

RDecl* NEnumElemVarDecl::GetROuter()
{
    return enumElem;
}

RIdentifier NEnumElemVarDecl::GetIdentifier()
{
    return RIdentifier { name, 0, {} };
}

RTypeDecl* NEnumElemVarDecl::GetTypeMember(const RName& name, size_t typeParamCount)
{
    return nullptr;
}

optional<RMember> NEnumElemVarDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RMember> NEnumElemVarDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

RType* NEnumElemVarDecl::GetDeclType(RTypeArguments& typeArgs, RFactory& factory)
{
    return declType->Apply(typeArgs, factory);
}

}