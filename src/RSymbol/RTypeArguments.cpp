#include "RTypeArguments.h"

#include <vector>

#include "RTypes.h"
#include "RFactory.h"

using namespace std;

namespace Citron {

RTypeArguments::RTypeArguments(std::vector<RType*>&& items, RFactory* factory, PrivateKey key)
    : items{move(items)}, factory{factory}
{
}

RTypeArguments* RTypeArguments::Remove(size_t count)
{
    return factory->MakeTypeArguments(span<RType*>(items.data() + count, items.size() - count));
}

RTypeArguments* RTypeArguments::Apply(RTypeArguments* typeArgs)
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