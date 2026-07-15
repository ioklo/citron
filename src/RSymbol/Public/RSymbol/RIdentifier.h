#pragma once

#include <vector>

#include "Infra/Hash.h"

#include "RNames.h"

namespace Citron {

class RType;

struct RIdentifier
{
    RName name;
    std::vector<RType*> paramIds;

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
        Citron::hash_combine(s, identifier.name);
        Citron::hash_combine(s, identifier.paramIds);
        return s;
    }
};

}