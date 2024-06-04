module;
#include "Macros.h"

export module Citron.Names;

import <variant>;
import <string>;
import <vector>;

namespace Citron {

// 통합 Identifier 세 부분으로 구성된다
// 이름 name, 타입 파라미터 개수 type parameter count, func parameterIds

export class NormalName
{
    std::string text;

public:
    NormalName(std::string&& text);
    DECLARE_DEFAULTS(NormalName)
};

export class ReservedName
{
    std::string text;

public:
    ReservedName(std::string&& text);
    DECLARE_DEFAULTS(ReservedName)
};

export class LambdaName
{
    int index;

public:
    LambdaName(int index);
    DECLARE_DEFAULTS(LambdaName)
};

export class ConstructorParamName
{
    int index;
    std::string paramText;
public:
    ConstructorParamName(int index, std::string&& paramText);
    DECLARE_DEFAULTS(ConstructorParamName)
};

export using Name = std::variant<
    NormalName,
    ReservedName,
    LambdaName,
    ConstructorParamName
>;

export Name Copy(const Name& name)
{
    return std::visit([](auto&& name) { return Name(name.Copy()); }, name);
}

}