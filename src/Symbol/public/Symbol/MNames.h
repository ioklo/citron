#pragma once
#include "SymbolConfig.h"

#include <string>
#include <variant>

#include <Infra/Defaults.h>

namespace Citron 
{

// 통합 Identifier 세 부분으로 구성된다
// 이름 name, 타입 파라미터 개수 type parameter count, func parameterIds
class MName_Normal
{
    std::string text;

public:
    SYMBOL_API MName_Normal(std::string&& text);
    DECLARE_DEFAULTS(SYMBOL_API, MName_Normal)
};

class MName_Reserved
{
    std::string text;

public:
    SYMBOL_API MName_Reserved(std::string&& text);
    DECLARE_DEFAULTS(SYMBOL_API, MName_Reserved)
};

class MName_Lambda
{
    int index;

public:
    SYMBOL_API MName_Lambda(int index);
    DECLARE_DEFAULTS(SYMBOL_API, MName_Lambda)
};

class MName_ConstructorParam
{
    int index;
    std::string paramText;
public:
    SYMBOL_API MName_ConstructorParam(int index, std::string&& paramText);
    DECLARE_DEFAULTS(SYMBOL_API, MName_ConstructorParam)
};

using MName = std::variant<
    MName_Normal,
    MName_Reserved,
    MName_Lambda,
    MName_ConstructorParam
>;

SYMBOL_API MName Copy(const MName& name);

}