#pragma once

#include <variant>
#include <optional>
#include <vector>

namespace Citron {

struct QReturnPassingMode_Void {};
struct QReturnPassingMode_Direct {}; // direct인 경우, context의 리턴 slot을 사용한다
struct QReturnPassingMode_Indirect { size_t index; }; // 리턴값을 가리키는 포인터가 함수의 인자 중 몇 번째인지
using QReturnPassingMode = std::variant<QReturnPassingMode_Void, QReturnPassingMode_Direct, QReturnPassingMode_Indirect>;

struct QThisPassingMode_None {};
struct QThisPassingMode_Handle { size_t index; };
struct QThisPassingMode_Ptr { size_t index; };
using QThisPassingMode = std::variant<QThisPassingMode_None, QThisPassingMode_Handle, QThisPassingMode_Ptr>;

enum class QParamPassingMode
{
    Direct,   // slot 자체가 값을 담는다
    Indirect, // slot이 값을 담는 포인터를 담는다
    Ref,      // slot이 참조하는 객체를 담는다 (예: [in]T&)
    Forward,  // contains lvalue, rvalue flags
    Params,   // count, params
};

struct QFuncInfo
{
    QReturnPassingMode returnPassingMode;
    QThisPassingMode thisPassingMode;
    size_t explicitArgStartIndex;
    std::vector<QParamPassingMode> paramPassingModes;
};

} // namespace Citron