#pragma once
#include "IR0Config.h"
#include <Infra/Defaults.h>
#include <Infra/Hash.h>
#include <string>
#include <variant>

namespace Citron 
{

// 통합 Identifier 세 부분으로 구성된다
// 이름 name, 타입 파라미터 개수 type parameter count, func parameterIds
struct RNormalName
{
    std::string text;
    bool operator==(const RNormalName& other) const noexcept = default;
};

struct RReservedName
{
    std::string text;
    bool operator==(const RReservedName& other) const noexcept = default;
};

struct RLambdaName
{
    int index;
    bool operator==(const RLambdaName& other) const noexcept = default;
};

struct RConstructorParamName
{
    int index;
    std::string paramText;
    bool operator==(const RConstructorParamName& other) const noexcept = default;
};

using RName = std::variant<
    RNormalName,
    RReservedName,
    RLambdaName,
    RConstructorParamName
>;

IR0_API RName Copy(const RName& name);

}

namespace std {

template<>
struct hash<Citron::RNormalName>
{
    std::size_t operator()(const Citron::RNormalName& name) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, name.text);
        return s;
    }
};

template<>
struct hash<Citron::RReservedName>
{
    std::size_t operator()(const Citron::RReservedName& name) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, name.text);
        return s;
    }
};

template<>
struct hash<Citron::RLambdaName>
{
    std::size_t operator()(const Citron::RLambdaName& name) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, name.index);
        return s;
    }
};

template<>
struct hash<Citron::RConstructorParamName>
{
    std::size_t operator()(const Citron::RConstructorParamName& name) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, name.index);
        Citron::hash_combine(s, name.paramText);
        return s;
    }
};

}