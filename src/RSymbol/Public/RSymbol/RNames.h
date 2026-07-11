#pragma once
#include "RSymbolConfig.h"

#include "Infra/Ref.h"

#include <string>
#include <variant>

#include "Infra/Hash.h"

namespace Citron {

struct RName_None 
{
    bool operator==(const RName_None& other) const noexcept = default;
};

// 통합 Identifier 세 부분으로 구성된다
// 이름 name, 타입 파라미터 개수 type parameter count, func parameterIds
struct RName_Normal
{
    std::string text;
    bool operator==(const RName_Normal& other) const noexcept = default;
};

enum class RName_ReservedName
{
    Enumerator,
    GetEnumerator,
    Next,
    RawItem,
    This,
    Return,
    Ctor,
    Dtor,
};

struct RName_Reserved
{
    RName_ReservedName name;
    bool operator==(const RName_Reserved& other) const noexcept = default;
};

struct RName_Lambda
{
    size_t index;
    bool operator==(const RName_Lambda& other) const noexcept = default;
};

struct RName_CtorParam
{
    size_t index;
    std::string paramText;
    bool operator==(const RName_CtorParam& other) const noexcept = default;
};

class RName
{
    using Variant = std::variant<
        RName_None, // for Root Namespace
        RName_Normal,
        RName_Reserved,
        RName_Lambda,
        RName_CtorParam>;

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

    void hash_combine(std::size_t& seed) const noexcept
    {
        Citron::hash_combine(seed, v);
    }

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }
};

RSYMBOL_API std::string RName_ReservedNameToString(InRef<RName_ReservedName> name);

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
struct hash<Citron::RName_None>
{
    std::size_t operator()(const Citron::RName_None& name) const noexcept
    {
        return 0;
    }
};


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
struct hash<Citron::RName_Reserved>
{
    std::size_t operator()(const Citron::RName_Reserved& name) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, name.name);
        return s;
    }
};

template<>
struct hash<Citron::RName_Lambda>
{
    std::size_t operator()(const Citron::RName_Lambda& name) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, name.index);
        return s;
    }
};

template<>
struct hash<Citron::RName_CtorParam>
{
    std::size_t operator()(const Citron::RName_CtorParam& name) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, name.index);
        Citron::hash_combine(s, name.paramText);
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