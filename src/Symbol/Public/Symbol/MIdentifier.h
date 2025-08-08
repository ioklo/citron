#pragma once

#include <vector>
#include "Infra/Hash.h"

#include "MNames.h"

namespace Citron {

class MType;

struct MIdentifier
{
    MName name;
    int typeParamCount;
    std::vector<MType*> paramIds;
};

} // namespace Citron

namespace std {

template<>
struct hash<Citron::MIdentifier>
{
    size_t operator()(const Citron::MIdentifier& identifier) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, identifier.name);
        Citron::hash_combine(s, identifier.typeParamCount);
        Citron::hash_combine(s, identifier.paramIds);
        return s;
    }
};
} // namespace std