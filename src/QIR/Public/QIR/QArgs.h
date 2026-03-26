#pragma once
#include <string>
#include <variant>

namespace Citron {

// 메모리 참조가 가능한 값에 대한 추상화
struct QArg_Slot
{
    size_t index;

public:
    QArg_Slot(size_t index)
        : index{index}
    {
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

// Exp의 결과 값으로 가능 한 것들
using QArg_Input = std::variant<QArg_Slot, QArg_ConstBool, QArg_ConstInt32>;

} // Citron