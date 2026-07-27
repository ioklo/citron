#pragma once
#include <string>

namespace Citron {

struct RTypeIdentifier
{
    std::string text;
    bool operator==(const RTypeIdentifier& other) const = default;
};


} // namespace Citron

namespace std { 

template<>
struct hash<Citron::RTypeIdentifier>
{
    size_t operator()(const Citron::RTypeIdentifier& key) const
    {
        return std::hash<std::string>()(key.text);
    }
};

} // namespace std