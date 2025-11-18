#pragma once
#include <string>
#include <variant>

namespace Citron {

// 값을 지칭하는 구조
struct QValue_Named
{
    std::string name;
};

struct QValue_Local
{
    size_t index;
};

struct QValue_ConstBool
{
    bool value;
};

struct QValue_ConstInteger
{
    int value;
};

struct QValue_String
{
    std::string value;
};

using QValue = std::variant<
    QValue_Local,
    QValue_Named, 
    QValue_ConstBool,
    QValue_ConstInteger,
    QValue_String
>;

} // Citron