#include "QBodyContext.h"

#include "Infra/Unreachable.h"
#include "Infra/Exceptions.h"

#include "QIR/QInsts.h"
#include "QIR/QValues.h"

using namespace std;

namespace Citron::IR0IR1Translator {

void QBlockWriter::Allocate()
{
    switch(state)
    {
    case QBlockWriterState::NotAllocated:
        curBlock = qFactory->MakeQBlock("entry");
        state = QBlockWriterState::CanWrite;
        return;

    case QBlockWriterState::CanWrite:
    case QBlockWriterState::EndOfBlock:
        assert(false);
    }
}

QBlock* QBlockWriter::GetEntryBlock()
{
    assert(entryBlock);
    return entryBlock;
}

void QBlockWriter::AddInstInternal(QInst&& inst)
{
    switch (state)
    {
    case QBlockWriterState::NotAllocated:
        Allocate();
        assert(state == QBlockWriterState::CanWrite);
        curBlock->AddInst(std::move(inst));
        return;

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
    pendingBlocks.push_back(newBlock);
    return newBlock;
}

void QBlockWriter::CompleteBlock(QTermInst&& termInst)
{
    switch(state)
    {
    case QBlockWriterState::NotAllocated:
        AddInstInternal(visit([](auto&& termInst) -> QInst { return termInst; }, termInst));
        state = QBlockWriterState::EndOfBlock;
        return;

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
    case QBlockWriterState::NotAllocated:
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

bool QBlockWriter::Verify()
{
    if (state != QBlockWriterState::EndOfBlock) return false;
    if (!pendingBlocks.empty()) return false;

    return true;
}

QBodyContext::QBodyContext(QFactoryPtr& qFactory)
    : QBlockWriter{}
    , qFactory {qFactory}
{   
}


QValue QBodyContext::AddIntrinsic(QInst_IntrinsicKind kind, std::vector<QValue>&& args)
{
    auto lv = NewValue();
    QBlockWriter::AddInst(QInst_Intrinsic{kind, lv, move(args)});
    return lv;
}

QValue_Named QBodyContext::NewValue()
{
    throw NotImplementedException{};
}

size_t QBodyContext::GetExpTypeSize(MExp* exp)
{
    throw NotImplementedException{};
}

} // Citron::IR0IR1Translator
