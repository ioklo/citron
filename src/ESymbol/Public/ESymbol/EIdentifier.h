#pragma once

#include <vector>
#include "Infra/Hash.h"

#include "ENames.h"

namespace Citron {

class EType;

struct EIdentifier
{
    EName name;
    int typeParamCount;
    std::vector<EType*> paramIds;
};

} // namespace Citron

namespace std {

template<>
struct hash<Citron::EIdentifier>
{
    size_t operator()(const Citron::EIdentifier& identifier) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, identifier.name);
        Citron::hash_combine(s, identifier.typeParamCount);
        Citron::hash_combine(s, identifier.paramIds);
        return s;
    }
};
} // namespace std