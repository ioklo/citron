#pragma once
#include "SymbolConfig.h"

#include <string>
#include <variant>

#include <Infra/Defaults.h>

namespace Citron 
{

// 통합 Identifier 세 부분으로 구성된다
// 이름 name, 타입 파라미터 개수 type parameter count, func parameterIds
class MNormalName
{
    std::string text;

public:
    SYMBOL_API MNormalName(std::string&& text);
    DECLARE_DEFAULTS(SYMBOL_API, MNormalName)
};

class MReservedName
{
    std::string text;

public:
    SYMBOL_API MReservedName(std::string&& text);
    DECLARE_DEFAULTS(SYMBOL_API, MReservedName)
};

class MLambdaName
{
    int index;

public:
    SYMBOL_API MLambdaName(int index);
    DECLARE_DEFAULTS(SYMBOL_API, MLambdaName)
};

class MConstructorParamName
{
    int index;
    std::string paramText;
public:
    SYMBOL_API MConstructorParamName(int index, std::string&& paramText);
    DECLARE_DEFAULTS(SYMBOL_API, MConstructorParamName)
};

using MName = std::variant<
    MNormalName,
    MReservedName,
    MLambdaName,
    MConstructorParamName
>;

SYMBOL_API MName Copy(const MName& name);

}