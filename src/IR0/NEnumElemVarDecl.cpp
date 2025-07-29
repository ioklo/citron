#include "NEnumElemVarDecl.h"

#include "Infra/Exceptions.h"

#include "RTypes.h"
#include "NEnumDecl.h"

using namespace std;

namespace Citron {

NEnumElemVarDecl::NEnumElemVarDecl(std::weak_ptr<NEnumElemDecl> enumElem, const std::string& name)
    : enumElem(move(enumElem))
    , name(name)
{
}

void Citron::NEnumElemVarDecl::InitDeclType(RTypePtr&& declType)
{
    this->declType = move(declType);
}

NDecl* NEnumElemVarDecl::GetNOuter()
{
    return enumElem.lock().get();
}

RDecl* NEnumElemVarDecl::GetROuter()
{
    return enumElem.lock().get();
}

RIdentifier NEnumElemVarDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

optional<RMember> NEnumElemVarDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

std::optional<RMember> NEnumElemVarDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

RTypePtr NEnumElemVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return declType->Apply(typeArgs, factory);
}

}