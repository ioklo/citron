#include "RTypeArguments.h"

#include <vector>

#include "RTypes.h"
#include "IR0Factory.h"

using namespace std;

namespace Citron {

RTypeArguments::RTypeArguments(const std::vector<RType*>& items)
    : items(items)
{
}

size_t RTypeArguments::GetCount()
{
    return items.size();
}

RType* RTypeArguments::Get(int i)
{
    return items[i];
}

RTypeArguments* RTypeArguments::Apply(RTypeArguments& typeArgs, IR0Factory& factory)
{
    vector<RType*> appliedItems;
    appliedItems.reserve(items.size());

    for(auto& item : items)
    {
        auto* appliedItem = item->Apply(typeArgs, factory);
        appliedItems.push_back(appliedItem);
    }

    return factory.MakeTypeArguments(appliedItems);
}

} // namespace Citron