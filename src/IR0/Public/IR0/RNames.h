#pragma once
#include "IR0Config.h"
#include <Infra/Defaults.h>
#include <string>
#include <variant>

namespace Citron 
{

// 통합 Identifier 세 부분으로 구성된다
// 이름 name, 타입 파라미터 개수 type parameter count, func parameterIds
class RNormalName
{
    std::string text;

public:
    IR0_API RNormalName(std::string&& text);
    DECLARE_DEFAULTS(IR0_API, RNormalName)
};

class RReservedName
{
    std::string text;

public:
    IR0_API RReservedName(std::string&& text);
    DECLARE_DEFAULTS(IR0_API, RReservedName)
};

class RLambdaName
{
    int index;

public:
    IR0_API RLambdaName(int index);
    DECLARE_DEFAULTS(IR0_API, RLambdaName)
};

class RConstructorParamName
{
    int index;
    std::string paramText;
public:
    IR0_API RConstructorParamName(int index, std::string&& paramText);
    DECLARE_DEFAULTS(IR0_API, RConstructorParamName)
};

using RName = std::variant<
    RNormalName,
    RReservedName,
    RLambdaName,
    RConstructorParamName
>;

IR0_API RName Copy(const RName& name);

}