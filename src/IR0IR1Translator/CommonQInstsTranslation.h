#pragma once
#include <expected>
#include <memory>
#include <vector>
#include <optional>

#include "RSymbol/RFuncParameter.h"
#include "MIR/MArgument.h"
#include "QIR/QArgs.h"
#include "QIR/QInsts.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

enum class MExp_CallIntrinsicKind;
enum class MInitExp_CallIntrinsicKind;

class RFuncDecl;
class RTypeArguments;

struct MStmt_Scope;
struct MInitExp_String;
struct QTranslationContexts;

template<typename T>
struct QEmitState;
enum class QParamPassingMode;
struct QFuncInfo;
struct MqIntrinsicInfo;
using QLocResult = std::variant<struct QLocResult_Slot, struct QLocResult_Ptr>;

std::expected<QEmitState<std::optional<size_t>>, DiagPtr> HandleIntrinsicCall(MqIntrinsicInfo& intrinsicInfo, std::optional<size_t> o_destSlotIndex, RTypeArguments* typeArgs, std::vector<MArgument>& mArgs, QTranslationContexts& contexts);
std::expected<QEmitState<std::optional<size_t>>, DiagPtr> HandleCall(RFuncDecl* decl, RTypeArguments* typeArgs, std::optional<size_t> o_destSlotIndex, MLoc* o_instance, std::vector<MArgument>& mArgs, QTranslationContexts& contexts);

QArg_CallArg MakeAddrCallArg(QLocResult& locResult, QTranslationContexts& contexts);
size_t MakePtrSlot(QLocResult& locResult, QTranslationContexts& contexts);

std::expected<QEmitState<QArg_CallArg>, DiagPtr> TranslateMArgumentToQInsts(MArgument& arg, QParamPassingMode passingMode, QTranslationContexts& contexts);
std::expected<QEmitState<void>, DiagPtr> TranslateMArgumentsToQInsts(std::vector<QArg_CallArg>& qArgs, std::vector<MArgument>& mArgs, QFuncInfo& funcInfo, QTranslationContexts& contexts);

std::expected<QEmitState<void>, DiagPtr> TranslateMInitExp_StringToQInsts(MInitExp_String* exp, std::optional<size_t> destSlot, QTranslationContexts& contexts);

std::expected<QEmitState<void>, DiagPtr> HandleInlineBlock(MStmt_Scope* scope, std::optional<size_t> o_destSlotIndex, QTranslationContexts& contexts);

} // namespace Citron