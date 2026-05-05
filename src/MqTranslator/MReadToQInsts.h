#pragma once
#include <memory>
#include <expected>
#include "MIR/MRead.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct MRead_Loc;
struct MRead_Exp;
struct MqTranslationContexts;

struct MqReadResult_Slot { size_t slotIndex; }; // local var
struct MqReadResult_Ptr { size_t slotIndex; };  // ptr
struct MqReadResult_ConstBool { bool value; };
struct MqReadResult_ConstInt32 { int value; };

using MqReadResult = std::variant<MqReadResult_Slot, MqReadResult_Ptr, MqReadResult_ConstBool, MqReadResult_ConstInt32>;

template<typename T>
struct MqEmitState;

std::expected<MqEmitState<MqReadResult>, DiagPtr> TranslateMRead_LocToQInsts(MRead_Loc& mReadLoc, MqTranslationContexts& contexts);
std::expected<MqEmitState<MqReadResult>, DiagPtr> TranslateMReadToQInsts(MRead& mRead, MqTranslationContexts& contexts);

//visit([](auto& result) -> ResultType {
//    using T = remove_cvref_t<decltype(result)>;
//    if constexpr (same_as<T, MqReadResult_Slot>)
//    else if constexpr (same_as<T, MqReadResult_Ptr>)
//    else if constexpr (same_as<T, MqReadResult_ConstBool>)
//    else if constexpr (same_as<T, MqReadResult_ConstInt32>)
//    else static_assert(false)
//}, *e_result);

} // namespace Citron
