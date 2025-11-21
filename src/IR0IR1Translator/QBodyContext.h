#pragma once

#include <string>
#include <memory>
#include <unordered_map>
#include <span>

#include "RSymbol/RNames.h"

#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QFuncBody.h"

namespace Citron {

class MExp;
class RType;
struct QStackSlot;
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
    void AddInstInternal(QInst&& inst);

public:
    QBlock* AddBlock(std::string&& debugText);

    template<typename TQInst>
        requires std::convertible_to<TQInst, QInst> && !std::convertible_to<TQInst, QTermInst>
    void AddInst(TQInst&& inst) { AddInstInternal(std::move(inst)); }
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

class QBodyContext : QBlockWriter
{
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;
    size_t regCounter;
    size_t slotCounter;

    // 함수의 스택 변수
    QBlock* entryBlock;
    QBlock* bodyBlock;
    std::vector<QStackSlot> stackSlots;
    std::vector<QScope> scopes;

public:
    QBodyContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory);

    QType* GetIntrinsicResultType(QInst_IntrinsicKind kind);

    QBlock* AddBlock(std::string&& debugText) { return QBlockWriter::AddBlock(std::move(debugText)); }
    template<typename TQInst, typename... TArgs> 
        requires std::convertible_to<TQInst, QInst>
            && !std::convertible_to<TQInst, QTermInst>
            && !std::same_as<TQInst, QInst_Intrinsic>
    void AddInst(TQInst&& inst) { QBlockWriter::AddInst(std::move(inst)); }
    QArg AddIntrinsic(QInst_IntrinsicKind kind, std::vector<QArg>&& args);
    void AddIntrinsicVoid(QInst_IntrinsicKind kind, std::vector<QArg>&& args);
    void CompleteBlock(QTermInst&& termInst) { QBlockWriter::CompleteBlock(std::move(termInst)); }
    void SetCurBlock(QBlock* block) { QBlockWriter::SetCurBlock(block); }

public:
    QFactory& GetQFactory() { return *qFactory; }

    QBlock* GetEntryBlock() { return entryBlock; }
    size_t GetRegisterCount() { return regCounter; }

    QArg NewContainer(QType* qType);
    QType* GetMExpQType(MExp* mExp);    
    size_t GetMExpTypeSize(MExp* exp);
    size_t GetRTypeSize(RType* rType);

    QType* MakeQType(RType* rType);
    QType_Class* MakeQStringType();

    QArg_StackSlot AddLocalVar(RType* type, const RName& name);
    QArg_StackSlot GetLocalVar(const RName& name);

    QArg_StackSlot NewStackSlot(QType* qType);
    std::span<QStackSlot> GetStackSlots() { return stackSlots; }

    void CompleteFunc();

};

} // namespace IR0IR1Translator


} // namespace Citron