#include "RGenericsComponent.h"

#include <cassert>
#include "Infra/Ref.h"
#include "RTypeParamDecl.h"

using namespace std;

namespace Citron {

RGenericsComponent::RGenericsComponent()
{
}

void RGenericsComponent::InitTypeParams(vector<RTypeParamDecl*>&& typeParams)
{
    this->o_typeParams = move(typeParams);
}

RTypeParamDecl* RGenericsComponent::GetTypeParam(size_t index)
{
    assert(o_typeParams);
    return (*o_typeParams)[index];
}

RTypeDecl* RGenericsComponent::GetTypeMember(InRef<RName> name, size_t typeParamCount)
{
    assert(o_typeParams);

    if (typeParamCount != 0) return nullptr;

    for (auto* typeParam : *o_typeParams)
    {
        if (typeParam->GetName() == *name)
            return typeParam;
    }
    return nullptr;
}

optional<RDeclRes> RGenericsComponent::ResolveTypeParam(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    assert(o_typeParams);

    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    for (auto* typeParam : *o_typeParams)
    {
        if (typeParam->GetName() == *name)
            return RDeclRes_TypeVar(typeParam);
    }

    return nullopt;
}

} // namespace Citron