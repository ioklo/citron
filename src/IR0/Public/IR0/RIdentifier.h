#pragma once

#include <memory>
#include <vector>
#include <Infra/Hash.h>

#include "RNames.h"


namespace Citron {

using RTypePtr = std::shared_ptr<class RType>;

struct RIdentifier
{
    RName name;
    size_t typeParamCount;
    std::vector<RTypePtr> paramIds;

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
        Citron::hash_combine(s, identifier.typeParamCount);
        Citron::hash_combine(s, identifier.paramIds);
        return s;
    }
};

}