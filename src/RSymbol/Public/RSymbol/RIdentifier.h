#pragma once
#include <vector>
#include <string>
#include "Infra/Hash.h"
#include "RNames.h"

namespace Citron {

struct RIdentifier
{
    std::string text;
    bool operator==(const RIdentifier& other) const = default;
};

}

namespace std {

template<>
struct hash<Citron::RIdentifier>
{
    size_t operator()(const Citron::RIdentifier& identifier) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, identifier.text);
        return s;
    }
};

}