#include "RTypeArguments.h"

#include <vector>

#include "RTypes.h"
#include "RFactory.h"

using namespace std;

namespace Citron {

RTypeArguments::RTypeArguments(const std::vector<RType*>& items, RFactory* factory)
    : items{items}, factory{factory}
{
}

size_t RTypeArguments::GetCount()
{
    return items.size();
}

RType* RTypeArguments::Get(size_t i)
{
    return items[i];
}

RTypeArguments* RTypeArguments::Apply(RTypeArguments& typeArgs)
{
    vector<RType*> appliedItems;
    appliedItems.reserve(items.size());

    for(auto& item : items)
    {
        auto* appliedItem = item->Apply(typeArgs);
        appliedItems.push_back(appliedItem);
    }

    return factory->MakeTypeArguments(appliedItems);
}

} // namespace Citron