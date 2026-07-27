#pragma once
#include "RSymbolConfig.h"
#include <string>
#include <variant>
#include "Infra/Ref.h"
#include "Infra/Hash.h"

namespace Citron {

struct RName_Normal
{
    std::string text;
    bool operator==(const RName_Normal& other) const noexcept = default;
};

class RName
{
    using Variant = std::variant<
        RName_Normal
    >;
    Variant v;

public: 
    static RName Normal(std::string text) { return RName{RName_Normal{std::move(text)}}; }

    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, RName>) && std::constructible_from<Variant, T&&>
    RName(T&& t) : v{std::forward<T>(t)} {}
    
    bool operator==(const RName& name) const noexcept
    {
        return v == name.v;
    }

    RSYMBOL_API std::string ToString();
    RName_Normal* TryGetNormal() { return std::get_if<RName_Normal>(&v); }

    void hash_combine(std::size_t& seed) const noexcept
    {
        Citron::hash_combine(seed, v);
    }

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }
};

namespace RNames {

RSYMBOL_API extern RName Enumerator;
RSYMBOL_API extern RName GetEnumerator;
RSYMBOL_API extern RName Next;
RSYMBOL_API extern RName RawItem;
RSYMBOL_API extern RName _this; // "this"
RSYMBOL_API extern RName _return; // "return"

} // namespace RNames

} // namespace Citron

namespace std {

template<>
struct hash<Citron::RName_Normal>
{
    std::size_t operator()(const Citron::RName_Normal& name) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, name.text);
        return s;
    }
};

template<>
struct hash<Citron::RName>
{
    std::size_t operator()(const Citron::RName& name) const noexcept
    {
        size_t s = 0;
        name.hash_combine(s);
        return s;
    }
};

} // namespace std