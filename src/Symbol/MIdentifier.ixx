module;
#include <vector>
#include <memory>

export module Citron.MDecls:MIdentifier;

import :MNames;
import Citron.Hash;

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
        return s;
    }
};
} // namespace std