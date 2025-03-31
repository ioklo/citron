export module Citron.RDecls:RIdentifier;

import <memory>;
import <vector>;

import Citron.Hash;

import :RNames;

namespace Citron {

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export struct RIdentifier
{
    RName name;
    size_t typeParamCount;
    std::vector<RTypePtr> paramIds;

    bool operator==(const RIdentifier& other) const = default;
};

}

namespace std {

export template<>
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