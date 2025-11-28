#pragma once

#include <string>
#include <memory>
#include <unordered_map>
#include <span>
#include <optional>

#include "RSymbol/RNames.h"

#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QFuncBody.h"

namespace Citron {

class MExp;
class RType;
class RTypeArguments;
struct QSlotInfo;
using RFactoryPtr = std::shared_ptr<class RFactory>;

enum class QInst_IntrinsicKind;

namespace IR0IR1Translator {

enum class QBlockWriterState
{   
    CanWrite,
    EndOfBlock,
};

class QBlockWriter
{
    QBlockWriterState state;
    QBlock* curBlock;
    std::vector<QBlock*> blocks;
    std::vector<QBlock*> pendingBlocks;
    QFactoryPtr qFactory;

public:
    QBlockWriter(const QFactoryPtr& qFactory, std::string&& blockName);

private:
    void EmitInstInternal(QInst&& inst);

public:
    QBlock* AddBlock(std::string&& debugText);

    template<typename TQInst>
        requires std::convertible_to<TQInst, QInst> && (!std::convertible_to<TQInst, QTermInst>)
    void EmitInst(TQInst&& inst) { EmitInstInternal(std::move(inst)); }
    void CompleteBlock(QTermInst&& termInst);

    QBlock* GetCurBlock() { return curBlock; }
    void SetCurBlock(QBlock* block);
    void Verify();
};

struct QLocalVarInfo
{
    size_t slotIndex;
    std::string name;
    QType* qType;
};

struct QScope
{
    // "a_16" -> slotIndex
    std::unordered_map<RName, QLocalVarInfo> localVarInfos;
};

struct QIntrinsicResultType_Slot { QType* qType; };
struct QIntrinsicKindResult_Void {};
using QIntrinsicResultType = std::variant<
    QIntrinsicResultType_Slot,
    QIntrinsicKindResult_Void>;

class QBodyContext : QBlockWriter
{
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;

    // 함수의 스택 변수
    QBlock* entryBlock;
    QBlock* bodyBlock;
    std::vector<QSlotInfo> slotInfos;
    std::vector<QScope> scopes;

public:
    QBodyContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory);

    QIntrinsicResultType GetIntrinsicResultType(QInst_IntrinsicKind kind);

    QBlock* AddBlock(std::string&& debugText) { return QBlockWriter::AddBlock(std::move(debugText)); }
    template<typename TQInst, typename... TArgs> 
        requires std::convertible_to<TQInst, QInst>
            && (!std::convertible_to<TQInst, QTermInst>)
            && (!std::same_as<TQInst, QInst_Intrinsic>)
    void EmitInst(TQInst&& inst) { QBlockWriter::EmitInst(std::move(inst)); }
    void EmitIntrinsic(QInst_IntrinsicKind kind, std::optional<QArg_Slot> oDest, std::vector<QArg_Input>&& args);
    void CompleteBlock(QTermInst&& termInst) { QBlockWriter::CompleteBlock(std::move(termInst)); }
    void SetCurBlock(QBlock* block) { QBlockWriter::SetCurBlock(block); }

public:
    QBlock* GetEntryBlock() { return entryBlock; }
    QType* GetMExpQType(MExp* mExp);    
    size_t GetQTypeSize(QType* qType);

    QType* GetQTypeFromRType(RType* rType);
    QType_Class* GetStringQType();
    QType* GetBoolQType();
    QType* GetIntQType();
    QType* GetPtrQType();

    size_t AddLocalVar(RType* type, const RName& name, std::optional<size_t> oArgIndex);
    size_t GetLocalVarSlotIndex(const RName& name);

    QArg_Slot NewSlot(QType* qType);
    std::span<QSlotInfo> GetStackSlotInfos() { return slotInfos; }

    QArg_Slot NewSlotForMExp(MExp* exp);

    void CompleteFunc();

    QType* GetReturnQType(RFuncDecl* rFuncDecl, RTypeArguments& typeArgs);
    bool IsVoidQType(QType* qType);
    
};

} // namespace IR0IR1Translator


} // namespace Citron