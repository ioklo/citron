#pragma once
#include "ESymbolConfig.h"

#include <string>
#include <variant>
#include "Infra/Hash.h"


#define DECLARE_DEFAULTS(linkage, className) \
    className(const className&) = delete; \
    linkage className(className&&); \
    className& operator=(const className&) = delete; \
    linkage className& operator=(className&&); \
    linkage ~className(); \
    linkage className Copy() const;

namespace Citron
{

// 통합 Identifier 세 부분으로 구성된다
// 이름 name, 타입 파라미터 개수 type parameter count, func parameterIds
class EName_Normal
{
public:
    std::string text;

public:
    ESYMBOL_API EName_Normal(std::string&& text);
    DECLARE_DEFAULTS(ESYMBOL_API, EName_Normal)
};

class EName_Reserved
{
public:
    std::string text;

public:
    ESYMBOL_API EName_Reserved(std::string&& text);
    DECLARE_DEFAULTS(ESYMBOL_API, EName_Reserved)
};

class EName_Lambda
{
public:
    int index;

public:
    ESYMBOL_API EName_Lambda(int index);
    DECLARE_DEFAULTS(ESYMBOL_API, EName_Lambda)
};

class EName_CtorParam
{
public:
    int index;
    std::string paramText;
public:
    ESYMBOL_API EName_CtorParam(int index, std::string&& paramText);
    DECLARE_DEFAULTS(ESYMBOL_API, EName_CtorParam)
};

using EName = std::variant<
    EName_Normal,
    EName_Reserved,
    EName_Lambda,
    EName_CtorParam
>;

ESYMBOL_API EName Copy(const EName& name);

}


namespace std
{
    template<>
    struct hash<Citron::EName_Normal>
    {
        std::size_t operator()(const Citron::EName_Normal& name) const noexcept
        {
            size_t s = 0;
            Citron::hash_combine(s, name.text);
            return s;
        }
    };

    template<>
    struct hash<Citron::EName_Reserved>
    {
        std::size_t operator()(const Citron::EName_Reserved& name) const noexcept
        {
            size_t s = 0;
            Citron::hash_combine(s, name.text);
            return s;
        }
    };

    template<>
    struct hash<Citron::EName_Lambda>
    {
        std::size_t operator()(const Citron::EName_Lambda& name) const noexcept
        {
            size_t s = 0;
            Citron::hash_combine(s, name.index);
            return s;
        }
    };

    template<>
    struct hash<Citron::EName_CtorParam>
    {
        std::size_t operator()(const Citron::EName_CtorParam& name) const noexcept
        {
            size_t s = 0;
            Citron::hash_combine(s, name.index);
            Citron::hash_combine(s, name.paramText);
            return s;
        }
    };
} // namespace std