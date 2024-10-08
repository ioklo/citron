#include "RTypeArguments.h"

#include "RType.h"
#include "RTypeFactory.h"

using namespace std;

namespace Citron {

RTypeArguments::RTypeArguments(const std::vector<RTypePtr>& items)
    : items(items)
{
}

size_t RTypeArguments::GetCount()
{
    return items.size();
}

const RTypePtr& RTypeArguments::Get(int i)
{
    return items[i];
}

RTypeArgumentsPtr RTypeArguments::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    vector<RTypePtr> appliedItems;
    appliedItems.reserve(items.size());

    for(auto& item : items)
    {
        auto appliedItem = item->Apply(typeArgs, factory);
        appliedItems.push_back(std::move(appliedItem));
    }

    return factory.MakeTypeArguments(appliedItems);
}

} // namespace Citron