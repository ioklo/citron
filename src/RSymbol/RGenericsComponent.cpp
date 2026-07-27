#include "RGenericsComponent.h"

#include <cassert>
#include "Infra/Ref.h"
#include "RTypeParam.h"
#include "RNames.h"

using namespace std;

namespace Citron {

RGenericsComponent::RGenericsComponent()
{
}

void RGenericsComponent::InitTypeParams(vector<RTypeParam*>&& typeParams)
{
    this->o_typeParams = move(typeParams);
}

RTypeParam* RGenericsComponent::GetTypeParam(size_t index)
{
    assert(o_typeParams);
    return (*o_typeParams)[index];
}

RTypeParam* RGenericsComponent::GetTypeParam(InRef<RName> name)
{
    assert(o_typeParams);

    for (auto* typeParam : *o_typeParams)
    {
        if (typeParam->GetName() == *name)
            return typeParam;
    }

    return nullptr;
}

} // namespace Citron