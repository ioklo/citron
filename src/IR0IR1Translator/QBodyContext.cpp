#include "QBodyContext.h"

#include <sstream>
#include <format>

#include "Infra/Unreachable.h"
#include "Infra/Exceptions.h"

#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"

#include "MIR/MExp.h"

#include "QIR/QInsts.h"
#include "QIR/QValues.h"

using namespace std;

namespace Citron::IR0IR1Translator {

QBlockWriter::QBlockWriter(const QFactoryPtr& qFactory, string&& firstBlockName)
    : qFactory{qFactory}
{
    auto* firstBlock = qFactory->MakeQBlock(move(firstBlockName));
    this->curBlock = firstBlock;
    this->blocks.push_back(firstBlock);
    this->state = QBlockWriterState::CanWrite;
}

void QBlockWriter::AddInstInternal(QInst&& inst)
{
    switch (state)
    {
    case QBlockWriterState::CanWrite:
        curBlock->AddInst(std::move(inst));
        return;

    case QBlockWriterState::EndOfBlock:
        assert(false);
    }

    unreachable();
}

QBlock* QBlockWriter::AddBlock(string&& debugText)
{
    auto* newBlock = qFactory->MakeQBlock(move(debugText));
    blocks.push_back(newBlock);
    pendingBlocks.push_back(newBlock);
    return newBlock;
}

void QBlockWriter::CompleteBlock(QTermInst&& termInst)
{
    switch(state)
    {
    case QBlockWriterState::CanWrite:
        AddInstInternal(visit([](auto&& termInst) -> QInst { return termInst; }, termInst));
        state = QBlockWriterState::EndOfBlock;
        return;

    case QBlockWriterState::EndOfBlock:
        assert(false);
        return;
    }
}

void QBlockWriter::SetCurBlock(QBlock* block)
{
    switch (state)
    {
    case QBlockWriterState::CanWrite:
        assert(false);

    case QBlockWriterState::EndOfBlock:
        for (size_t i = 0, count = pendingBlocks.size(); i < count; i++)
        {
            if (pendingBlocks[i] == block)
            {   
                // swap
                if (size_t lastIndex = count - 1; i != lastIndex)
                {
                    auto* t = pendingBlocks[lastIndex];
                    pendingBlocks[lastIndex] = pendingBlocks[i];
                    pendingBlocks[i] = t;
                }

                pendingBlocks.pop_back();
                curBlock = block;
                state = QBlockWriterState::CanWrite;
                return;
            }
        }
        assert(false);
    }
}

void QBlockWriter::Verify()
{
    assert(state == QBlockWriterState::EndOfBlock);
    assert(pendingBlocks.empty());
}

QBodyContext::QBodyContext(const RFactoryPtr& rFactory, const QFactoryPtr& qFactory)
    : rFactory{rFactory}
    , qFactory{qFactory}
    , QBlockWriter{qFactory, "body"}
    , valueCounter{0}
    , entryBlock{nullptr}
{
    scopes.emplace_back();
    bodyBlock = QBlockWriter::GetCurBlock();
}

QValue QBodyContext::AddIntrinsic(QInst_IntrinsicKind kind, std::vector<QValue>&& args)
{
    auto lv = NewValue();
    QBlockWriter::AddInst(QInst_Intrinsic{kind, lv, move(args)});
    return lv;
}

QValue_Named QBodyContext::NewValue()
{   
    return {format("v{}", valueCounter++)};
}

size_t QBodyContext::GetMExpTypeSize(MExp* exp)
{
    auto* type = exp->GetType(*rFactory);
    return GetRTypeSize(type);
}

size_t QBodyContext::GetRTypeSize(RType* type)
{
    if (type == rFactory->MakeBoolType())
        return 1;

    if (type == rFactory->MakeIntType())
        return 4;

    throw NotImplementedException{};

    /*struct Visitor
    {
        using ResultType = size_t;
        size_t Visit(RType_Void*) { return 0; }
        size_t Visit(RType_Struct* structType)
        {
            rFactory->

        }

    } visitor{} ;

    return Accept(visitor, type);*/
    
}

QValue_Local QBodyContext::AddLocalVar(RType* type, const RName& name)
{
    size_t index = localVars.size();
    localVars.emplace_back(name, type);

    scopes.back().varNames[name] = index;
    return QValue_Local{index};
}

QValue_Local QBodyContext::GetLocalVar(const RName& name)
{
    return QValue_Local{scopes.back().varNames[name]};
}


void QBodyContext::CompleteFunc()
{
    // make entry
    entryBlock = QBlockWriter::AddBlock("entry");
    QBlockWriter::SetCurBlock(entryBlock);

    for(size_t i = 0, count = localVars.size(); i < count; i++)
    {
        size_t typeSize = GetRTypeSize(localVars[i].type);
        QBlockWriter::AddInst(QInst_Alloc{QValue_Local{i}, typeSize});
    }

    QBlockWriter::CompleteBlock(QInst_Jump{bodyBlock});

    QBlockWriter::Verify();
}


} // Citron::IR0IR1Translator
