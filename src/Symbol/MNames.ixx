module;
#include "Macros.h"

export module Citron.Symbols:MNames;

import <variant>;
import <string>;
import <vector>;

namespace Citron 
{

// 통합 Identifier 세 부분으로 구성된다
// 이름 name, 타입 파라미터 개수 type parameter count, func parameterIds
export class MNormalName
{
    std::string text;

public:
    MNormalName(std::string&& text);
    DECLARE_DEFAULTS(MNormalName)

    MNormalName(std::string&& text);
};

export class MReservedName
{
    std::string text;

public:
    MReservedName(std::string&& text);
    DECLARE_DEFAULTS(MReservedName)
};

export class MLambdaName
{
    int index;

public:
    MLambdaName(int index);
    DECLARE_DEFAULTS(MLambdaName)
};

export class MConstructorParamName
{
    int index;
    std::string paramText;
public:
    MConstructorParamName(int index, std::string&& paramText);
    DECLARE_DEFAULTS(MConstructorParamName)
};

export using MName = std::variant<
    MNormalName,
    MReservedName,
    MLambdaName,
    MConstructorParamName
>;

export MName Copy(const MName& name);

}