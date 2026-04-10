#pragma once
#include <expected>
#include <memory>
#include <optional>

#include "MIR/MArgument.h"
#include "QIR/QArgs.h"
#include "QIR/QInsts.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

enum class MExp_CallIntrinsicKind;
enum class MInitExp_CallIntrinsicKind;

struct MStmt_Scope;
struct MInitExp_String;
struct QTranslationContexts;

template<typename T>
struct QEmitState;

using QLocResult = std::variant<struct QLocResult_Slot, struct QLocResult_Ptr>;

struct QIntrinsicInfo
{
    QInst_IntrinsicKind kind;
    RType* type;
};

size_t MakePtrSlot(QLocResult& locResult, QTranslationContexts& contexts);
size_t MakePtrSlot(size_t slotIndex, QTranslationContexts& contexts);

std::expected<QEmitState<QArg_Input>, DiagPtr> TranslateMArgumentToQInsts(MArgument& arg, QTranslationContexts& contexts);
std::expected<QEmitState<std::vector<QArg_Input>>, DiagPtr> TranslateMArgumentsToQInsts(std::vector<MArgument>& mArgs, QTranslationContexts& contexts);

QIntrinsicInfo* GetIntrinsicInfo(MExp_CallIntrinsicKind kind, QTranslationContexts& contexts);
QIntrinsicInfo* GetIntrinsicInfo(MInitExp_CallIntrinsicKind kind, QTranslationContexts& contexts);

std::expected<QEmitState<void>, DiagPtr> TranslateMInitExp_StringToQInsts(MInitExp_String* exp, std::optional<size_t> destSlot, QTranslationContexts& contexts);

std::expected<QEmitState<void>, DiagPtr> HandleInlineBlock(MStmt_Scope* scope, std::optional<size_t> o_destSlotIndex, QTranslationContexts& contexts);

} // namespace Citron