#pragma once
#include "RSymbolConfig.h"
#include <string>
#include <string_view>
#include <span>
#include <variant>
#include "Infra/Hash.h"
#include "Infra/Ref.h"
#include "RAppliedDecl.h"

namespace Citron {

class RName;
struct RFuncParameter;
class RTraitDecl;
class RTypeArguments;
class RDecl;
class RDeclKey;

class RDeclKey
{
    std::string value; // root 면 ""

public:
    static RDeclKey RootNamespace() { return RDeclKey{""}; }
    RSYMBOL_API static RDeclKey Normal(InRef<RName> name);
    RSYMBOL_API static RDeclKey Func(InRef<RName> name, std::span<RFuncParameter> funcParams);
    RSYMBOL_API static RDeclKey Ctor(std::span<RFuncParameter> funcParams);
    RSYMBOL_API static RDeclKey Dtor();
    RSYMBOL_API static RDeclKey ImplTrait(RDecl* target, RAppliedDecl<RTraitDecl> appliedTraitDecl);

    RDeclKey(std::string value) : value{std::move(value)} {}
    bool operator==(const RDeclKey& other) const = default;

    void hash_combine(std::size_t& seed) const noexcept
    {
        Citron::hash_combine(seed, value);
    }

    std::string_view GetValue() { return value; }

};

} // namespace Citron

namespace std {

template<>
struct hash<Citron::RDeclKey>
{
    size_t operator()(const Citron::RDeclKey& key) const
    {
        size_t seed = 0;
        key.hash_combine(seed);
        return seed;
    }
};

} // namespace std