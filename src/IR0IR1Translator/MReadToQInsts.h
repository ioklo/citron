#pragma once
#include <memory>
#include <expected>
#include "MIR/MRead.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct MRead_Loc;
struct MRead_Exp;
struct QTranslationContexts;

struct QReadResult_Slot { size_t slotIndex; }; // local var
struct QReadResult_Ptr { size_t slotIndex; }; // ptr
struct QReadResult_ConstBool { bool value; };
struct QReadResult_ConstInt32 { int value; };

using QReadResult = std::variant<QReadResult_Slot, QReadResult_Ptr, QReadResult_ConstBool, QReadResult_ConstInt32>;
using QReadResult_Value = std::variant<QReadResult_Slot, QReadResult_ConstBool, QReadResult_ConstInt32>;
using QReadResult_Place = std::variant<QReadResult_Slot, QReadResult_Ptr>;

std::expected<QReadResult_Place, DiagPtr> TranslateMRead_LocToQInsts(MRead_Loc& mReadLoc, QTranslationContexts& contexts);
std::expected<QReadResult_Value, DiagPtr> TranslateMRead_ExpToQInsts(MRead_Exp& mReadExp, QTranslationContexts& contexts);
std::expected<QReadResult, DiagPtr> TranslateMReadToQInsts(MRead& mRead, QTranslationContexts& contexts);

//visit([](auto& result) -> ResultType {
//    using T = remove_cvref_t<decltype(result)>;
//    if constexpr (same_as<T, QReadResult_Slot>)
//    else if constexpr (same_as<T, QReadResult_Ptr>)
//    else if constexpr (same_as<T, QReadResult_ConstBool>)
//    else if constexpr (same_as<T, QReadResult_ConstInt32>)
//    else static_assert(false)
//}, *e_result);

} // namespace Citron
