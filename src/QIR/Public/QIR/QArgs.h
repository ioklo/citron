#pragma once
#include <string>
#include <variant>

namespace Citron {

struct QArg_Value_Slot { size_t index; };
struct QArg_Value_ConstBool { bool value; };
struct QArg_Value_ConstInt32 { int value; };
using QArg_Value = std::variant<QArg_Value_Slot, QArg_Value_ConstBool, QArg_Value_ConstInt32>;

struct QArg_Addr_OfSlot { size_t index; }; // slot의 주소
struct QArg_Addr_PtrSlot { size_t index; }; // slot에 주소가 들어있다
using QArg_Addr = std::variant<QArg_Addr_OfSlot, QArg_Addr_PtrSlot>;

struct QArg_CallArg_Slot { size_t index; };
struct QArg_CallArg_ConstBool { bool value; };
struct QArg_CallArg_ConstInt32 { int value; };
struct QArg_CallArg_AddrOfSlot { size_t index; };
using QArg_CallArg = std::variant<QArg_CallArg_Slot, QArg_CallArg_ConstBool, QArg_CallArg_ConstInt32, QArg_CallArg_AddrOfSlot>;

struct QArg_Dest { size_t index; };

} // Citron