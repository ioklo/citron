export module Citron.MNames;

import "SymbolConfig.h";

import <string>;
import <variant>;

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
export class MName_Normal
{
    std::string text;

public:
    SYMBOL_API MName_Normal(std::string&& text);
    DECLARE_DEFAULTS(SYMBOL_API, MName_Normal)
};

export class MName_Reserved
{
    std::string text;

public:
    SYMBOL_API MName_Reserved(std::string&& text);
    DECLARE_DEFAULTS(SYMBOL_API, MName_Reserved)
};

export class MName_Lambda
{
    int index;

public:
    SYMBOL_API MName_Lambda(int index);
    DECLARE_DEFAULTS(SYMBOL_API, MName_Lambda)
};

export class MName_CtorParam
{
    int index;
    std::string paramText;
public:
    SYMBOL_API MName_CtorParam(int index, std::string&& paramText);
    DECLARE_DEFAULTS(SYMBOL_API, MName_CtorParam)
};

export using MName = std::variant<
    MName_Normal,
    MName_Reserved,
    MName_Lambda,
    MName_CtorParam
>;

export SYMBOL_API MName Copy(const MName& name);

}