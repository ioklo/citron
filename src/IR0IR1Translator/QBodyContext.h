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
struct QStackSlotInfo;
using RFactoryPtr = std::shared_ptr<class RFactory>;

struct QArg_Register;
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
    // "a_16" -> regIndex
    std::unordered_map<RName, QLocalVarInfo> localVarInfos;
};

struct QIntrinsicKindResultType_Register { QRegisterType type; };
struct QIntrinsicKindResultType_StackSlot { QType* qType; };
struct QIntrinsicKindResultType_Void {};
using QIntrinsicKindResultType = std::variant<
    QIntrinsicKindResultType_Register,
    QIntrinsicKindResultType_StackSlot,
    QIntrinsicKindResultType_Void>;

class QBodyContext : QBlockWriter
{
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;

    // 함수의 스택 변수
    QBlock* entryBlock;
    QBlock* bodyBlock;
    std::vector<QStackSlotInfo> slotInfos;
    std::vector<QRegisterInfo> registerInfos;
    std::vector<QScope> scopes;

public:
    QBodyContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory);

    QIntrinsicKindResultType GetIntrinsicResultType(QInst_IntrinsicKind kind);

    QBlock* AddBlock(std::string&& debugText) { return QBlockWriter::AddBlock(std::move(debugText)); }
    template<typename TQInst, typename... TArgs> 
        requires std::convertible_to<TQInst, QInst>
            && (!std::convertible_to<TQInst, QTermInst>)
            && (!std::same_as<TQInst, QInst_Intrinsic>)
    void EmitInst(TQInst&& inst) { QBlockWriter::EmitInst(std::move(inst)); }
    void EmitIntrinsic(QInst_IntrinsicKind kind, std::optional<QArg_Loc> oResult, std::vector<QArg_Input>&& args);
    void CompleteBlock(QTermInst&& termInst) { QBlockWriter::CompleteBlock(std::move(termInst)); }
    void SetCurBlock(QBlock* block) { QBlockWriter::SetCurBlock(block); }

public:
    QFactory& GetQFactory() { return *qFactory; }

    QBlock* GetEntryBlock() { return entryBlock; }
    
    std::optional<QRegisterType> GetRegisterType(QType* qType);

    QArg_Register NewRegister(QRegisterType type);
    QType* GetMExpQType(MExp* mExp);    
    size_t GetQTypeSize(QType* qType);

    QType* MakeQType(RType* rType);
    QType_Class* MakeQStringType();

    QArg_StackSlot AddLocalVar(RType* type, const RName& name);
    QArg_StackSlot GetLocalVar(const RName& name);

    QArg_StackSlot NewStackSlot(QType* qType);
    std::span<QStackSlotInfo> GetStackSlotInfos() { return slotInfos; }
    std::span<QRegisterInfo> GetRegisterInfos() { return registerInfos; }

    QArg_Loc NewContainerForMExp(MExp* exp);

    void CompleteFunc();

};

} // namespace IR0IR1Translator


} // namespace Citron