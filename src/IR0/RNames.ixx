export module Citron.RDecls:RNames;

import "IR0Config.h";

import <string>;
import <variant>;

import Citron.Hash;

namespace Citron {

// 통합 Identifier 세 부분으로 구성된다
// 이름 name, 타입 파라미터 개수 type parameter count, func parameterIds
export struct RName_Normal
{
    std::string text;
    bool operator==(const RName_Normal& other) const noexcept = default;
};

export struct RName_Reserved
{
    std::string text;
    bool operator==(const RName_Reserved& other) const noexcept = default;
};

export struct RName_Lambda
{
    size_t index;
    bool operator==(const RName_Lambda& other) const noexcept = default;
};

export struct RName_CtorParam
{
    size_t index;
    std::string paramText;
    bool operator==(const RName_CtorParam& other) const noexcept = default;
};

export using RName = std::variant<
    RName_Normal,
    RName_Reserved,
    RName_Lambda,
    RName_CtorParam
>;

export IR0_API RName Copy(const RName& name);

namespace RNames {

export IR0_API extern RName Enumerator;
export IR0_API extern RName GetEnumerator;
export IR0_API extern RName Next;
export IR0_API extern RName RawItem;
export IR0_API extern RName _this; // "this"

} // namespace RNames

} // namespace Citron

namespace std {

export template<>
struct hash<Citron::RName_Normal>
{
    std::size_t operator()(const Citron::RName_Normal& name) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, name.text);
        return s;
    }
};

export template<>
struct hash<Citron::RName_Reserved>
{
    std::size_t operator()(const Citron::RName_Reserved& name) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, name.text);
        return s;
    }
};

export template<>
struct hash<Citron::RName_Lambda>
{
    std::size_t operator()(const Citron::RName_Lambda& name) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, name.index);
        return s;
    }
};

export template<>
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