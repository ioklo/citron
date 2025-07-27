module;
#include <vector>
#include <memory>
#include "Infra/Hash.h"

export module Citron.MDecls:MIdentifier;

import :MNames;

namespace Citron {

export class MType;
export using MTypePtr = std::shared_ptr<MType>;

export struct MIdentifier
{
    MName name;
    int typeParamCount;
    std::vector<MTypePtr> paramIds;
};

} // namespace Citron

namespace std {

export template<>
struct hash<Citron::MIdentifier>
{
    size_t operator()(const Citron::MIdentifier& identifier) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, identifier.name);
        Citron::hash_combine(s, identifier.typeParamCount);
        Citron::hash_combine(s, identifier.paramIds);

        //std::hash<std::vector<Citron::MTypePtr>> hasher;
        // s ^= hasher(identifier.paramIds) + 0x9e3779b9 + (s << 6) + (s >> 2);
        return s;
    }
};
} // namespace std