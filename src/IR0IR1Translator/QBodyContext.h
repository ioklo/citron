#pragma once

#include <string>
#include <memory>
#include <unordered_map>

#include "RSymbol/RNames.h"

#include "QIR/QFactory.h"
#include "QIR/QBlock.h"

namespace Citron {

class MExp;
class RType;
using RFactoryPtr = std::shared_ptr<class RFactory>;

struct QValue_Named;
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
    RName name;
    RType* type;
};

struct QScope
{
    // a -> 16
    std::unordered_map<RName, size_t> varNames;
};

class QBodyContext : QBlockWriter
{
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;
    int valueCounter;

    // 함수의 스택 변수
    QBlock* entryBlock;
    QBlock* bodyBlock;
    std::vector<QLocalVarInfo> localVars;
    std::vector<QScope> scopes;

public:
    QBodyContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory);

    QBlock* AddBlock(std::string&& debugText) { return QBlockWriter::AddBlock(std::move(debugText)); }
    template<typename TQInst, typename... TArgs> 
        requires std::convertible_to<TQInst, QInst>
            && !std::convertible_to<TQInst, QTermInst>
            && !std::same_as<TQInst, QInst_Intrinsic>
    void AddInst(TQInst&& inst) { QBlockWriter::AddInst(std::move(inst)); }
    QValue AddIntrinsic(QInst_IntrinsicKind kind, std::vector<QValue>&& args);
    void CompleteBlock(QTermInst&& termInst) { QBlockWriter::CompleteBlock(std::move(termInst)); }
    void SetCurBlock(QBlock* block) { QBlockWriter::SetCurBlock(block); }

public:
    QBlock* GetEntryBlock() { return entryBlock; }

    QValue_Named NewValue();
    size_t GetMExpTypeSize(MExp* exp);
    size_t GetRTypeSize(RType* type);

    QValue_Local AddLocalVar(RType* type, const RName& name);
    QValue_Local GetLocalVar(const RName& name);

    void CompleteFunc();

};

} // namespace IR0IR1Translator


} // namespace Citron