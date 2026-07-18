#pragma once
#include "RSymbolConfig.h"
#include <vector>

namespace Citron {

class RType;
class RFactory;

// 용어 정리
// C<T>.D<U, V>
// typeArgs(전체) = outerTypeArgs(최외각 멤버 제외) + memberTypeArgs(최외각 멤버)
// [T, U, V] = [T] + [U, V]
class RTypeArguments
{
    std::vector<RType*> items;
    RFactory* factory;

private:
    friend RFactory;
    RTypeArguments(std::vector<RType*>&& items, RFactory* factory);

public:
    size_t GetCount() { return items.size(); }
    RType* Get(size_t i) { return items[i]; }
    RSYMBOL_API RTypeArguments* Apply(RTypeArguments* typeArgs);
    RSYMBOL_API RTypeArguments* Remove(size_t count);
};



} // namespace Citron