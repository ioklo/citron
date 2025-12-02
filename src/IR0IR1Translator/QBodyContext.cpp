#include "QBodyContext.h"

#include <variant>
#include <format>
#include <ranges>

#include "Infra/Unreachable.h"
#include "Infra/Exceptions.h"
#include "Infra/Variants.h"

#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFuncDecl.h"

#include "MIR/MExp.h"

#include "QIR/QInsts.h"
#include "QIR/QArgs.h"
#include "QIR/QFactory.h"


using namespace std;

namespace Citron {

QBlockWriter::QBlockWriter(const QFactoryPtr& qFactory, string&& firstBlockName)
    : qFactory{qFactory}
{
    auto* firstBlock = qFactory->MakeQBlock(format("b{}_{}", blocks.size(), move(firstBlockName)));
    this->curBlock = firstBlock;
    this->blocks.push_back(firstBlock);
    this->state = QBlockWriterState::CanWrite;
}

void QBlockWriter::EmitInstInternal(QInst&& inst)
{
    switch (state)
    {
    case QBlockWriterState::CanWrite:
        curBlock->EmitInst(std::move(inst));
        return;

    case QBlockWriterState::EndOfBlock:
        assert(false);
    }

    unreachable();
}

QBlock* QBlockWriter::AddBlock(string&& debugText)
{
    auto* newBlock = qFactory->MakeQBlock(format("b{}_{}", blocks.size(), move(debugText)));
    blocks.push_back(newBlock);
    pendingBlocks.push_back(newBlock);
    return newBlock;
}

void QBlockWriter::CompleteBlock(QTermInst&& termInst)
{
    switch(state)
    {
    case QBlockWriterState::CanWrite:
        EmitInstInternal(visit([](auto&& termInst) -> QInst { return termInst; }, termInst));
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
    , entryBlock{nullptr}
{
    scopes.emplace_back();
    bodyBlock = QBlockWriter::GetCurBlock();
}

// GetIntrinsicResultType
// 1. result = intrinsic kind, args, ...
// 2. intrinsic kind, &result, args, ...
// 3. intrinsic kind, args, ... (void)
QIntrinsicResultType QBodyContext::GetIntrinsicResultType(QInst_IntrinsicKind kind)
{
    switch (kind)
    {
    case QInst_IntrinsicKind::DebugPrint_Items: 
        return QIntrinsicKindResult_Void{};

    case QInst_IntrinsicKind::Command_Items: 
        return QIntrinsicKindResult_Void{};

    case QInst_IntrinsicKind::Alloc_Int: throw NotImplementedException{};

    case QInst_IntrinsicKind::Memcpy_Ptr_Ptr_Int:
        return QIntrinsicKindResult_Void{};

    case QInst_IntrinsicKind::NewList_Items: throw NotImplementedException{};
    case QInst_IntrinsicKind::GetListIterator_List: throw NotImplementedException{};

    case QInst_IntrinsicKind::LogicalNot_Bool:
        return QIntrinsicResultType_Slot{qFactory->MakeBoolType()};

    case QInst_IntrinsicKind::UnaryMinus_Int:
        return QIntrinsicResultType_Slot{qFactory->MakeIntType()};

    case QInst_IntrinsicKind::ToString_Bool:
    case QInst_IntrinsicKind::ToString_Int: 
    case QInst_IntrinsicKind::Add_String_String:
        return QIntrinsicResultType_Slot{qFactory->MakeStringType()};

    case QInst_IntrinsicKind::PrefixInc_Int: 
    case QInst_IntrinsicKind::PrefixDec_Int:
    case QInst_IntrinsicKind::PostfixInc_Int:
    case QInst_IntrinsicKind::PostfixDec_Int:

    case QInst_IntrinsicKind::Multiply_Int_Int:
    case QInst_IntrinsicKind::Divide_Int_Int:
    case QInst_IntrinsicKind::Modulo_Int_Int:
    case QInst_IntrinsicKind::Add_Int_Int:
    case QInst_IntrinsicKind::Subtract_Int_Int:
        return QIntrinsicResultType_Slot{qFactory->MakeIntType()};
    
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
        return QIntrinsicResultType_Slot{qFactory->MakeBoolType()};
    }

    throw NotImplementedException{};
}

void QBodyContext::EmitIntrinsic(QInst_IntrinsicKind kind, optional<QArg_Slot> oDest, std::vector<QArg_Input>&& args)
{
    auto resultType = GetIntrinsicResultType(kind);
    if (auto* slotResultType = get_if<QIntrinsicResultType_Slot>(&resultType))
    {
        QArg_Slot resultSlot = [this, &oDest, slotResultType] {
            if (oDest) return *oDest;
            return NewSlot(slotResultType->qType);
        }();

        QBlockWriter::EmitInst(QInst_Intrinsic{kind, resultSlot, move(args)});
    }
    else if (auto* voidResultType = get_if<QIntrinsicKindResult_Void>(&resultType))
    {
        assert(!oDest);
        QBlockWriter::EmitInst(QInst_Intrinsic{kind, nullopt, move(args)});
    }
    else unreachable();
}

QType* QBodyContext::GetMExpQType(MExp* mExp)
{
    return GetQTypeFromRType(mExp->GetType(*rFactory));
}

size_t QBodyContext::GetQTypeSize(QType* qType)
{
    // TODO: HARD CODED
    if (qType == qFactory->MakeBoolType())
        return 1;

    if (qType == qFactory->MakeIntType())
        return 4;

    if (qType == qFactory->MakeStringType())
        return sizeof(string);

    throw NotImplementedException{};

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

QType* QBodyContext::GetQTypeFromRType(RType* rType)
{
    // TODO: HARD CODED
    if (rType == rFactory->MakeVoidType())
        return qFactory->MakeVoidType();

    if (rType == rFactory->MakeBoolType())
        return qFactory->MakeBoolType();

    if (rType == rFactory->MakeIntType())
        return qFactory->MakeIntType();

    if (rType == rFactory->MakeStringType())
        return qFactory->MakeStringType();

    throw NotImplementedException{};
}

QType_Class* QBodyContext::GetStringQType()
{
    return qFactory->MakeStringType();
}

QType* QBodyContext::GetBoolQType()
{
    return qFactory->MakeBoolType();
}

QType* QBodyContext::GetIntQType()
{
    return qFactory->MakeIntType();
}

QType* QBodyContext::GetPtrQType()
{
    return qFactory->MakePtrType();
}

size_t QBodyContext::AddLocalVar(RType* rType, const RName& rName, optional<size_t> oArgIndex)
{   
    size_t slotIndex = slotInfos.size(); // 여기서의 index는 모든 named 변수의 index (local vars가 어디 들어있는지는 별개)
    auto name = format("%s{}_{}", slotIndex, RNameToString(rName));
    QType* qType = GetQTypeFromRType(rType);

    // 1. 함수 entry에서 할당할 목록에 추가
    slotInfos.emplace_back(qType, name, oArgIndex);
    
    // 2. 현재 스코프에 이름 추가
    scopes.back().localVarInfos[rName] = QLocalVarInfo{slotIndex, name, qType};

    return slotIndex;
}

size_t QBodyContext::GetLocalVarSlotIndex(const RName& name)
{   
    for (auto& scope : scopes | views::reverse)
    {
        auto i = scope.localVarInfos.find(name);
        if (i != scope.localVarInfos.end())
            return i->second.slotIndex;
    }

    assert(false);
    return (size_t)-1;
}

QArg_Slot QBodyContext::NewSlot(QType* qType)
{
    size_t slotIndex = slotInfos.size();
    std::string s = format("%s{}", slotIndex);
    slotInfos.emplace_back(qType, s, nullopt);
    return {slotIndex};
}

QArg_Slot QBodyContext::NewSlotForMExp(MExp* exp)
{
    auto* qType = GetMExpQType(exp);
    return NewSlot(qType);
}

void QBodyContext::CompleteFunc()
{
    // make entry
    entryBlock = QBlockWriter::AddBlock("entry");
    QBlockWriter::SetCurBlock(entryBlock);

    // entry를 일단은 살려둠

    QBlockWriter::CompleteBlock(QInst_Jump{bodyBlock});
    QBlockWriter::Verify();
}

QType* QBodyContext::GetReturnQType(RFuncDecl* rFuncDecl, RTypeArguments& typeArgs)
{
    auto* rType = rFuncDecl->GetReturnType(typeArgs, *rFactory);
    return GetQTypeFromRType(rType);
}

bool QBodyContext::IsVoidQType(QType* qType)
{
    return qType == qFactory->MakeVoidType();
}

void QBodyContext::PushScope()
{
    scopes.push_back(QScope{});
}

void QBodyContext::PopScope()
{
    scopes.pop_back();
}

} // Citron
