#include "QBodyContext.h"

#include <variant>
#include <format>

#include "Infra/Unreachable.h"
#include "Infra/Exceptions.h"
#include "Infra/Variants.h"

#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RNames.h"

#include "MIR/MExp.h"

#include "QIR/QInsts.h"
#include "QIR/QValues.h"
#include "QIR/QFactory.h"


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
    , regCounter{0}
    , entryBlock{nullptr}
{
    scopes.emplace_back();
    bodyBlock = QBlockWriter::GetCurBlock();
}

QType* QBodyContext::GetIntrinsicResultType(QInst_IntrinsicKind kind)
{
    switch (kind)
    {
    case QInst_IntrinsicKind::DebugPrint_Items: 
        return qFactory->MakeVoidType();
    case QInst_IntrinsicKind::Command_Items: 
        return qFactory->MakeVoidType();

    case QInst_IntrinsicKind::Alloc_Int: // ?
    case QInst_IntrinsicKind::NewList_Items: // ?
    case QInst_IntrinsicKind::GetListIterator_List: // ?
        throw NotImplementedException{};

    case QInst_IntrinsicKind::LogicalNot_Bool:
        return qFactory->MakeBoolType();

    case QInst_IntrinsicKind::UnaryMinus_Int:
        return qFactory->MakeIntType();

    case QInst_IntrinsicKind::ToString_Bool:
    case QInst_IntrinsicKind::ToString_Int: 
    case QInst_IntrinsicKind::Add_String_String:
        return qFactory->MakeStringType();

    case QInst_IntrinsicKind::PrefixInc_Int: 
    case QInst_IntrinsicKind::PrefixDec_Int:
    case QInst_IntrinsicKind::PostfixInc_Int:
    case QInst_IntrinsicKind::PostfixDec_Int:

    case QInst_IntrinsicKind::Multiply_Int_Int:
    case QInst_IntrinsicKind::Divide_Int_Int:
    case QInst_IntrinsicKind::Modulo_Int_Int:
    case QInst_IntrinsicKind::Add_Int_Int:
    case QInst_IntrinsicKind::Subtract_Int_Int:
        return qFactory->MakeIntType();
    
    
    case QInst_IntrinsicKind::LessThan_Int_Int:
    case QInst_IntrinsicKind::LessThan_String_String:
    case QInst_IntrinsicKind::GreaterThan_Int_Int:
    case QInst_IntrinsicKind::GreaterThan_String_String:
    case QInst_IntrinsicKind::LessThanOrEqual_Int_Int:
    case QInst_IntrinsicKind::LessThanOrEqual_String_String:
    case QInst_IntrinsicKind::GreaterThanOrEqual_Int_Int:
    case QInst_IntrinsicKind::GreaterThanOrEqual_String_String:
    case QInst_IntrinsicKind::Equal_Int_Int:
    case QInst_IntrinsicKind::Equal_Bool_Bool:
    case QInst_IntrinsicKind::Equal_String_String:
        return qFactory->MakeBoolType();
    }

    throw NotImplementedException{};
}

QArg_Register QBodyContext::AddIntrinsic(QInst_IntrinsicKind kind, std::vector<QArg>&& args)
{
    auto* qType = GetIntrinsicResultType(kind);
    auto resultReg = NewRegister(qType);
    QBlockWriter::AddInst(QInst_Intrinsic{kind, resultReg, move(args)});
    return resultReg;
}

QArg_Register QBodyContext::NewRegister(QType* qType)
{   
    // TODO: 일반적인 타입에 대해서 해야 한다
    if (qType == qFactory->MakeStringType())
    {
        // string은 레지스터가 아닌 메모리에 할당된다
        return AddBuffer(qType);
    }

    size_t regIndex = regCounter++;
    return {regIndex, qType};
}

QType* QBodyContext::GetMExpQType(MExp* mExp)
{
    return MakeQType(mExp->GetType(*rFactory));
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

string RNameToString(const RName& name)
{
    return visit(overloaded{
        [](const RName_Normal& n) { return n.text; },
        [](const RName_Reserved& n) { return format("${}", n.text); },
        [](const RName_Lambda& n) { return format("$$lambdaVar{}>", n.index); },
        [](const RName_CtorParam& n) { return format("$$ctor_{}", n.paramText); }
    }, name);
}

QType* QBodyContext::MakeQType(RType* rType)
{
    if (rType == rFactory->MakeVoidType())
        return qFactory->MakeVoidType();

    if (rType == rFactory->MakeBoolType())
        return qFactory->MakeBoolType();

    if (rType == rFactory->MakeIntType())
        return qFactory->MakeIntType();

    throw NotImplementedException{};
}

QType_Class* QBodyContext::MakeQStringType()
{
    return qFactory->MakeStringType();
}

QArg_Register QBodyContext::AddLocalVar(RType* rType, const RName& rName)
{   
    size_t regIndex = regCounter++; // 여기서의 index는 모든 named 변수의 index (local vars가 어디 들어있는지는 별개)
    QType* qType = MakeQType(rType);

    // 1. 함수 entry에서 할당할 목록에 추가
    stackAllocationInfos.emplace_back(regIndex, qType);

    // 2. 현재 스코프에 이름 추가
    scopes.back().localVarInfos[rName] = QLocalVarInfo{regIndex, qType};

    return {regIndex, qType};
}

QArg_Register QBodyContext::GetLocalVar(const RName& name)
{   
    auto localVarInfo = scopes.back().localVarInfos[name];

    return {localVarInfo.regIndex, localVarInfo.qType};
}

QArg_Register QBodyContext::AddBuffer(QType* qType)
{
    size_t regIndex = regCounter++;
    stackAllocationInfos.emplace_back(regIndex, qType);
    return {regIndex, qType};
}

void QBodyContext::CompleteFunc()
{
    // make entry
    entryBlock = QBlockWriter::AddBlock("entry");
    QBlockWriter::SetCurBlock(entryBlock);

    for(auto& stackAllocationInfo : stackAllocationInfos)
        QBlockWriter::AddInst(QInst_Alloc{QArg_Register{stackAllocationInfo.regIndex, stackAllocationInfo.qType}});

    QBlockWriter::CompleteBlock(QInst_Jump{bodyBlock});

    QBlockWriter::Verify();
}


} // Citron::IR0IR1Translator
