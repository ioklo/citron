#pragma once
#include <vector>
#include <utility>

namespace Citron {

template<typename TKey>
concept SmallMapCanCompareKey = requires(TKey&& key) { 
    { key == key } -> std::convertible_to<bool>; 
};

// Value에서 Key를 추출하는 버전
template<typename TKey, typename TValue> requires SmallMapCanCompareKey<TKey>
class SmallMap
{
    std::vector<std::pair<TKey, TValue>> data;

public:
    template<typename TTKey, typename TTValue>
    void Add(TTKey&& key, TTValue&& value)
    {
        data.emplace_back(std::forward<TTKey>(key), std::forward<TTValue>(value));
    }

    TValue* Find(auto& key) 
    { 
        auto it = find_if(data.begin(), data.end(), [&key](const auto& entry) { return entry.first == key; });
        return it != data.end() ? &(*it) : nullptr;
    }

    TValue& FindOrAdd(auto&& key, auto&&... args)
    {
        if (auto* value = Find(key)) return *value;
        data.emplace_back(std::forward<decltype(key)>(key), TValue{std::forward<decltype(args)>(args)...});
        return data.back().second;
    }
};


} // namespace Citron
