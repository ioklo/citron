#pragma once

#include <memory>
#include <vector>
#include <Infra/Hash.h>

#include "RNames.h"


namespace Citron {

class RTypeId;
using RTypeIdPtr = std::shared_ptr<RTypeId>;

struct RIdentifier
{
    RName name;
    int typeParamCount;
    std::vector<RTypeIdPtr> paramIds;

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