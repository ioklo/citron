#pragma once

#include <variant>
#include <optional>
#include <vector>

namespace Citron {

struct MqReturnPassingMode_Void {};
struct MqReturnPassingMode_Direct {}; // direct인 경우, context의 리턴 slot을 사용한다
struct MqReturnPassingMode_Indirect { size_t index; }; // 리턴값을 가리키는 포인터가 함수의 인자 중 몇 번째인지
using MqReturnPassingMode = std::variant<MqReturnPassingMode_Void, MqReturnPassingMode_Direct, MqReturnPassingMode_Indirect>;

struct MqThisPassingMode_None {};
struct MqThisPassingMode_Handle { size_t index; };
struct MqThisPassingMode_Ptr { size_t index; };
using MqThisPassingMode = std::variant<MqThisPassingMode_None, MqThisPassingMode_Handle, MqThisPassingMode_Ptr>;

enum class MqParamPassingMode
{
    Direct,   // slot 자체가 값을 담는다
    Indirect, // slot이 값을 담는 포인터를 담는다
    Ref,      // slot이 참조하는 객체를 담는다 (예: [in]T&)
    Forward,  // contains lvalue, rvalue flags
    Params,   // count, params
};

struct MqFuncInfo
{
    MqReturnPassingMode returnPassingMode;
    MqThisPassingMode thisPassingMode;
    size_t explicitArgStartIndex;
    std::vector<MqParamPassingMode> paramPassingModes;
};

} // namespace Citron