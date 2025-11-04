#pragma once

#include <string>
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"

namespace Citron {

class MExp;

struct QValue_Named;
enum class QInst_IntrinsicKind;

namespace IR0IR1Translator {

enum class QBlockWriterState
{
    NotAllocated,
    CanWrite,
    EndOfBlock,
};

class QBlockWriter
{
    QBlockWriterState state;
    QBlock* entryBlock;
    QBlock* curBlock;
    std::vector<QBlock*> blocks;
    std::vector<QBlock*> pendingBlocks;
    QFactoryPtr qFactory;

private:
    void Allocate();
    void AddInstInternal(QInst&& inst);

public:
    QBlock* AddBlock(std::string&& debugText);
    QBlock* GetEntryBlock();

    template<typename TQInst>
        requires std::convertible_to<TQInst, QInst> and !std::convertible_to<TQInst, QTermInst>
    void AddInst(TQInst&& inst) { AddInstInternal(std::move(inst)); }
    void CompleteBlock(QTermInst&& termInst);

    void SetCurBlock(QBlock* block);
    bool Verify();
};

class QBodyContext : QBlockWriter
{
    QFactoryPtr qFactory;

public:
    QBodyContext(QFactoryPtr& qFactory);

    QBlock* AddBlock(std::string&& debugText) { QBlockWriter::AddBlock(std::move(debugText)); }
    QBlock* GetEntryBlock() { return QBlockWriter::GetEntryBlock(); }
    template<typename TQInst, typename... TArgs> 
        requires std::convertible_to<TQInst, QInst>
            and !std::convertible_to<TQInst, QTermInst>
            and !std::same_as<TQInst, QInst_Intrinsic>
    void AddInst(TQInst&& inst) { QBlockWriter::AddInst(std::move(inst)); }
    QValue AddIntrinsic(QInst_IntrinsicKind kind, std::vector<QValue>&& args);
    void CompleteBlock(QTermInst&& termInst) { QBlockWriter::CompleteBlock(std::move(termInst)); }
    void SetCurBlock(QBlock* block) { QBlockWriter::SetCurBlock(block); }
    bool Verify() { return QBlockWriter::Verify(); }

    QValue_Named NewValue();
    size_t GetExpTypeSize(MExp* exp);
};

} // namespace IR0IR1Translator


} // namespace Citron