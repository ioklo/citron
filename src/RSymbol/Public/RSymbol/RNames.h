#pragma once

#include "RSymbolConfig.h"

#include <string>
#include <variant>

#include "Infra/Hash.h"

namespace Citron {

// 통합 Identifier 세 부분으로 구성된다
// 이름 name, 타입 파라미터 개수 type parameter count, func parameterIds
struct RName_Normal
{
    std::string text;
    bool operator==(const RName_Normal& other) const noexcept = default;
};

struct RName_Reserved
{
    std::string text;
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

using RName = std::variant<
    RName_Normal,
    RName_Reserved,
    RName_Lambda,
    RName_CtorParam
>;

RSYMBOL_API RName Copy(const RName& name);

namespace RNames {

RSYMBOL_API extern RName Enumerator;
RSYMBOL_API extern RName GetEnumerator;
RSYMBOL_API extern RName Next;
RSYMBOL_API extern RName RawItem;
RSYMBOL_API extern RName _this; // "this"

} // namespace RNames

RSYMBOL_API std::string RNameToString(const RName& name);

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
struct hash<Citron::RName_Reserved>
{
    std::size_t operator()(const Citron::RName_Reserved& name) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, name.text);
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



} // namespace std