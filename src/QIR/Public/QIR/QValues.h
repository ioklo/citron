#pragma once
#include <string>
#include <variant>

namespace Citron {

struct QType;

// 값을 지칭하는 구조
struct QArg_Register
{
    size_t index;
    QType* qType;

public:
    QArg_Register(size_t index, QType* qType)
        : index{index}, qType{qType} {
    }
};

struct QArg_ConstBool
{
    bool value;
};

struct QArg_ConstInt32
{
    int value;
};

using QArg = std::variant<
    QArg_Register, 
    QArg_ConstBool,
    QArg_ConstInt32
>;

} // Citron