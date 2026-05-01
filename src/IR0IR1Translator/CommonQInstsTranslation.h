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
struct MqTranslationContexts;

template<typename T>
struct MqEmitState;
enum class MqParamPassingMode;
struct MqFuncInfo;
struct MqIntrinsicInfo;
using QLocResult = std::variant<struct QLocResult_Slot, struct QLocResult_Ptr>;

std::expected<MqEmitState<std::optional<QLocResult>>, DiagPtr> HandleIntrinsicCall(MqIntrinsicInfo& intrinsicInfo, MqCreateTarget createTarget, RTypeArguments* typeArgs, std::vector<MArgument>& mArgs, MqTranslationContexts& contexts);
std::expected<MqEmitState<std::optional<QLocResult>>, DiagPtr> HandleCall(RFuncDecl* decl, RTypeArguments* typeArgs, MqCreateTarget createTarget, MLoc* o_instance, std::vector<MArgument>& mArgs, MqTranslationContexts& contexts);

QArg_CallArg MakeAddrCallArg(QLocResult& locResult, MqTranslationContexts& contexts);
std::optional<QArg_CallArg> MakeAddrCallArg(MqCreateTarget& createTarget, MqTranslationContexts& contexts);
std::optional<QArg_Addr> MakeQArg_Addr(MqCreateTarget& createTarget, MqTranslationContexts& contexts);

std::expected<MqEmitState<QArg_CallArg>, DiagPtr> TranslateMArgumentToQInsts(MArgument& arg, MqParamPassingMode passingMode, MqTranslationContexts& contexts);
std::expected<MqEmitState<void>, DiagPtr> TranslateMArgumentsToQInsts(std::vector<QArg_CallArg>& qArgs, std::vector<MArgument>& mArgs, MqFuncInfo& funcInfo, MqTranslationContexts& contexts);

std::expected<MqEmitState<void>, DiagPtr> TranslateMInitExp_StringToQInsts(MInitExp_String* exp, MqCreateTarget createTarget, MqTranslationContexts& contexts);

std::expected<MqEmitState<void>, DiagPtr> HandleInlineBlock(MStmt_Scope* scope, MqCreateTarget createTarget, MqTranslationContexts& contexts);

void UpdateCreateTarget_Value(RType* type, size_t slotIndex, MqCreateTarget createTarget, MqTranslationContexts& contexts);

void UpdateCreateTarget_Ptr(RType* type, size_t ptrSlotIndex, MqCreateTarget createTarget, MqTranslationContexts& contexts);

void UpdateCreateTarget_AddrOf(size_t slotIndex, MqCreateTarget createTarget, MqTranslationContexts& contexts);

void UpdateCreateTarget(RType* type, QArg_Value&& v, MqCreateTarget createTarget, MqTranslationContexts& contexts);


} // namespace Citron