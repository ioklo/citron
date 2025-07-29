#pragma once
#include "SymbolConfig.h"

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
class MName_Normal
{
public:
    std::string text;

public:
    SYMBOL_API MName_Normal(std::string&& text);
    DECLARE_DEFAULTS(SYMBOL_API, MName_Normal)
};

class MName_Reserved
{
public:
    std::string text;

public:
    SYMBOL_API MName_Reserved(std::string&& text);
    DECLARE_DEFAULTS(SYMBOL_API, MName_Reserved)
};

class MName_Lambda
{
public:
    int index;

public:
    SYMBOL_API MName_Lambda(int index);
    DECLARE_DEFAULTS(SYMBOL_API, MName_Lambda)
};

class MName_CtorParam
{
public:
    int index;
    std::string paramText;
public:
    SYMBOL_API MName_CtorParam(int index, std::string&& paramText);
    DECLARE_DEFAULTS(SYMBOL_API, MName_CtorParam)
};

using MName = std::variant<
    MName_Normal,
    MName_Reserved,
    MName_Lambda,
    MName_CtorParam
>;

SYMBOL_API MName Copy(const MName& name);

}


namespace std
{
    template<>
    struct hash<Citron::MName_Normal>
    {
        std::size_t operator()(const Citron::MName_Normal& name) const noexcept
        {
            size_t s = 0;
            Citron::hash_combine(s, name.text);
            return s;
        }
    };

    template<>
    struct hash<Citron::MName_Reserved>
    {
        std::size_t operator()(const Citron::MName_Reserved& name) const noexcept
        {
            size_t s = 0;
            Citron::hash_combine(s, name.text);
            return s;
        }
    };

    template<>
    struct hash<Citron::MName_Lambda>
    {
        std::size_t operator()(const Citron::MName_Lambda& name) const noexcept
        {
            size_t s = 0;
            Citron::hash_combine(s, name.index);
            return s;
        }
    };

    template<>
    struct hash<Citron::MName_CtorParam>
    {
        std::size_t operator()(const Citron::MName_CtorParam& name) const noexcept
        {
            size_t s = 0;
            Citron::hash_combine(s, name.index);
            Citron::hash_combine(s, name.paramText);
            return s;
        }
    };
} // namespace std