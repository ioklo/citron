#include "NGenericsComponent.h"
#include <cassert>
#include "NTypeParamDecl.h"

using namespace std;

namespace Citron {

NGenericsComponent::NGenericsComponent()
{
}

void NGenericsComponent::InitTypeParams(vector<NTypeParamDecl*>&& typeParams)
{
    this->o_typeParams = move(typeParams);
}

RTypeParamDecl* NGenericsComponent::GetTypeParam(size_t index)
{
    assert(o_typeParams);
    return (*o_typeParams)[index];
}

RTypeDecl* NGenericsComponent::GetTypeMember(const RName& name, size_t typeParamCount)
{
    assert(o_typeParams);

    if (typeParamCount != 0) return nullptr;

    for (auto* typeParam : *o_typeParams)
    {
        if (typeParam->GetName() == name)
            return typeParam;
    }
    return nullptr;
}

optional<RDeclRes> NGenericsComponent::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    assert(o_typeParams);

    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    for (auto* typeParam : *o_typeParams)
    {
        if (typeParam->GetName() == name)
            return RDeclRes_TypeVar(typeParam);
    }

    return nullopt;
}

} // namespace Citron