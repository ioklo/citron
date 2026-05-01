#pragma once
#include <memory>
#include <expected>
#include "MIR/MRead.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct MRead_Loc;
struct MRead_Exp;
struct MqTranslationContexts;

struct QReadResult_Slot { size_t slotIndex; }; // local var
struct QReadResult_Ptr { size_t slotIndex; };  // ptr
struct QReadResult_ConstBool { bool value; };
struct QReadResult_ConstInt32 { int value; };

using QReadResult = std::variant<QReadResult_Slot, QReadResult_Ptr, QReadResult_ConstBool, QReadResult_ConstInt32>;

template<typename T>
struct MqEmitState;

std::expected<MqEmitState<QReadResult>, DiagPtr> TranslateMRead_LocToQInsts(MRead_Loc& mReadLoc, MqTranslationContexts& contexts);
std::expected<MqEmitState<QReadResult>, DiagPtr> TranslateMReadToQInsts(MRead& mRead, MqTranslationContexts& contexts);

//visit([](auto& result) -> ResultType {
//    using T = remove_cvref_t<decltype(result)>;
//    if constexpr (same_as<T, QReadResult_Slot>)
//    else if constexpr (same_as<T, QReadResult_Ptr>)
//    else if constexpr (same_as<T, QReadResult_ConstBool>)
//    else if constexpr (same_as<T, QReadResult_ConstInt32>)
//    else static_assert(false)
//}, *e_result);

} // namespace Citron
