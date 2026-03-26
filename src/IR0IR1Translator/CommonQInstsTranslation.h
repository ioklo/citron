#pragma once
#include <expected>
#include <memory>
#include <optional>
#include "MIR/MExp.h"
#include "MIR/MArgument.h"
#include "QIR/QArgs.h"
#include "QIR/QInsts.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

struct MInitExp_String;
struct QTranslationContexts;

struct QIntrinsicInfo
{
    QInst_IntrinsicKind kind;
    RType* type;
};

std::expected<QArg_Input, DiagPtr> TranslateMArgumentToQInsts(MArgument& arg, QTranslationContexts& contexts);
std::expected<std::vector<QArg_Input>, DiagPtr> TranslateMArgumentsToQInsts(std::vector<MArgument>& mArgs, QTranslationContexts& contexts);

QIntrinsicInfo* GetIntrinsicInfo(MExp_CallIntrinsicKind kind, QTranslationContexts& contexts);

std::expected<void, DiagPtr> TranslateMInitExp_StringToQInsts(MInitExp_String* exp, std::optional<size_t> destSlot, QTranslationContexts& contexts);
std::expected<void, DiagPtr> TranslateMInitExp_StringToQInstsWithNewScope(MInitExp_String* exp, std::optional<size_t> destSlot, QTranslationContexts& contexts);

} // namespace Citron