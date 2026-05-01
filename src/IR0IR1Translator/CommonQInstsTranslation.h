#pragma once
#include <expected>
#include <memory>
#include <vector>
#include <optional>

#include "RSymbol/RFuncParameter.h"
#include "MIR/MArgument.h"
#include "QIR/QArgs.h"
#include "QIR/QInsts.h"
#include "MqCreateTarget.h"

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

std::expected<QEmitState<std::optional<QLocResult>>, DiagPtr> HandleIntrinsicCall(MqIntrinsicInfo& intrinsicInfo, MqCreateTarget createTarget, RTypeArguments* typeArgs, std::vector<MArgument>& mArgs, QTranslationContexts& contexts);
std::expected<QEmitState<std::optional<QLocResult>>, DiagPtr> HandleCall(RFuncDecl* decl, RTypeArguments* typeArgs, MqCreateTarget createTarget, MLoc* o_instance, std::vector<MArgument>& mArgs, QTranslationContexts& contexts);

QArg_CallArg MakeAddrCallArg(QLocResult& locResult, QTranslationContexts& contexts);
std::optional<QArg_CallArg> MakeAddrCallArg(MqCreateTarget& createTarget, QTranslationContexts& contexts);
std::optional<QArg_Addr> MakeQArg_Addr(MqCreateTarget& createTarget, QTranslationContexts& contexts);

std::expected<QEmitState<QArg_CallArg>, DiagPtr> TranslateMArgumentToQInsts(MArgument& arg, QParamPassingMode passingMode, QTranslationContexts& contexts);
std::expected<QEmitState<void>, DiagPtr> TranslateMArgumentsToQInsts(std::vector<QArg_CallArg>& qArgs, std::vector<MArgument>& mArgs, QFuncInfo& funcInfo, QTranslationContexts& contexts);

std::expected<QEmitState<void>, DiagPtr> TranslateMInitExp_StringToQInsts(MInitExp_String* exp, MqCreateTarget createTarget, QTranslationContexts& contexts);

std::expected<QEmitState<void>, DiagPtr> HandleInlineBlock(MStmt_Scope* scope, MqCreateTarget createTarget, QTranslationContexts& contexts);

void UpdateCreateTarget_Value(RType* type, size_t slotIndex, MqCreateTarget createTarget, QTranslationContexts& contexts);

void UpdateCreateTarget_Ptr(RType* type, size_t ptrSlotIndex, MqCreateTarget createTarget, QTranslationContexts& contexts);

void UpdateCreateTarget_AddrOf(size_t slotIndex, MqCreateTarget createTarget, QTranslationContexts& contexts);

void UpdateCreateTarget(RType* type, QArg_Value&& v, MqCreateTarget createTarget, QTranslationContexts& contexts);


} // namespace Citron