#pragma once
#include <string>
#include <variant>

namespace Citron {

struct QType;

// 중간 과정 계산을 위한 것들 (크기가 작은 것들)
struct QArg_Register
{
    size_t index;
    std::string name; // r{index}
    QType* qType;

public:
    QArg_Register(size_t index, std::string name, QType* qType)
        : index{index}, name{std::move(name)}, qType{qType} 
    {
    }
};

// stack의 값에 대한 추상화
// 레지스터 보다 좀 큰 단위의 것들. 메모리 참조가 가능하다. 
// 지역변수, 중간값 중 크기가 큰 것
struct QArg_StackSlot
{
    size_t index;
    std::string name; // s{index}_varName
    QType* qType;

public:
    QArg_StackSlot(size_t index, std::string name, QType* qType)
        : index{index}, name{std::move(name)}, qType{qType} 
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

using QArg = std::variant<
    QArg_Register, 
    QArg_StackSlot, 
    QArg_ConstBool,
    QArg_ConstInt32
>;

} // Citron