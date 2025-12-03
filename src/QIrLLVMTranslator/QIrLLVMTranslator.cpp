#pragma warning(disable:4244 4722 4267 4146 4624)
#include "QIrLLVMTranslator.h"

#include <format>

#include <llvm/IR/LLVMContext.h>
#include <llvm/IR/Module.h>
#include <llvm/IR/BasicBlock.h>
#include <llvm/IR/IRBuilder.h>

#include "Infra/Variants.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RDecl.h"
#include "NSymbol/NFuncDecl.h"

#include "QIR/QFactory.h"
#include "QIR/QData.h"
#include "QIR/QFuncBody.h"
#include "QIR/QBlock.h"

using namespace std;

namespace Citron {

struct LDataImpl
{   
    llvm::Module _module;
};

struct LContextImpl
{
    llvm::LLVMContext context;
    RFactoryPtr rFactory;
    QFactoryPtr qFactory;
    llvm::Type* stringType = nullptr;
};

llvm::Type* GetType(RType* rType, LContextImpl& contextImpl)
{
    auto& context = contextImpl.context;
    auto& rFactory = *contextImpl.rFactory;

    // TODO: HARD CODED
    if (rType == rFactory.MakeVoidType())
        return llvm::Type::getVoidTy(context);

    else if (rType == rFactory.MakeBoolType())
        return llvm::Type::getInt1Ty(context);

    else if (rType == rFactory.MakeIntType())
        return llvm::Type::getInt32Ty(context);

    // string 을 무슨 타입으로 해야 하는지
    // 1) 현재 VC++ 컴파일러 옵션으로 sizeof(string)를 갖고 있는 타입으로
    else if (rType == rFactory.MakeStringType())
    {
        if (contextImpl.stringType)
            return contextImpl.stringType;

        llvm::Type* byteTy = llvm::Type::getInt8Ty(context);
        llvm::ArrayType* bufTy = llvm::ArrayType::get(byteTy, sizeof(string));

        contextImpl.stringType = llvm::StructType::create({bufTy}, "string");
        return contextImpl.stringType;
    }
    else throw NotImplementedException{};
}

llvm::Type* GetType(QType* qType, LContextImpl& contextImpl)
{
    auto& context = contextImpl.context;
    auto& qFactory = *contextImpl.qFactory;

    // TODO: HARD CODED
    if (qType == qFactory.MakeVoidType())
        return llvm::Type::getVoidTy(context);

    else if (qType == qFactory.MakeBoolType())
        return llvm::Type::getInt1Ty(context);

    else if (qType == qFactory.MakeIntType())
        return llvm::Type::getInt32Ty(context);

    // string 을 무슨 타입으로 해야 하는지
    // 1) 현재 VC++ 컴파일러 옵션으로 sizeof(string)를 갖고 있는 타입으로
    else if (qType == qFactory.MakeStringType())
    {
        if (contextImpl.stringType)
            return contextImpl.stringType;

        llvm::Type* byteTy = llvm::Type::getInt8Ty(context);
        llvm::ArrayType* bufTy = llvm::ArrayType::get(byteTy, sizeof(string));

        contextImpl.stringType = llvm::StructType::create({bufTy}, "string");
        return contextImpl.stringType;
    }
    else throw NotImplementedException{};
}

// TODO: 공통 라이브러리로 빼기
string RNameToString2(const RName& name)
{
    return visit(overloaded{
        [](const RName_Normal& n) { return n.text; },
        [](const RName_Reserved& n) { return format("${}", n.text); },
        [](const RName_Lambda& n) { return format("$$lambdaVar{}>", n.index); },
        [](const RName_CtorParam& n) { return format("$$ctor_{}", n.paramText); }
    }, name);
}

template <typename FolderTy, typename InserterTy>
llvm::Value* GetValue(QArg_Input& qInput, QType* qType, span<llvm::Value*> slotValues, LContextImpl& lContextImpl, llvm::IRBuilder<FolderTy, InserterTy>& builder)
{
    return visit([&builder, slotValues, qType, &lContextImpl](auto& qInput) -> llvm::Value*
    {
        using T = remove_cvref_t<decltype(qInput)>;
        if constexpr (same_as<T, QArg_ConstBool>)
        {
            return llvm::ConstantInt::getBool(lContextImpl.context, qInput.value);
        }
        else if constexpr (same_as<T, QArg_ConstInt32>)
        {
            auto* lInt32Type = llvm::Type::getInt32Ty(lContextImpl.context);
            return llvm::ConstantInt::get(lInt32Type, qInput.value);
        }
        else if constexpr (same_as<T, QArg_Slot>)
        {
            auto* lValueType = GetType(qType, lContextImpl);
            return builder.CreateLoad(lValueType, slotValues[qInput.index]);
        }
        else static_assert(false);
    }, qInput);
}


// 파일 하나에 대해서만 하는거
LData* TranslateQDataToLLVMData(QData* qData, LContext& lContext)
{
    auto& lContextImpl = *lContext.impl;
    llvm::Module _module{"MyModule", lContextImpl.context};

    for (auto& qFuncBody : qData->GetAllBodies())
    {   
        auto rFuncReturn = qFuncBody.nFuncDecl->GetUnboundFuncReturn();
        auto* lRetType = visit([&lContextImpl](auto& ret) -> llvm::Type* {
            using T = remove_cvref_t<decltype(ret)>;
            if constexpr (same_as<T, RFuncReturn_ForCtor>)
            {   
                // void
                return llvm::Type::getVoidTy(lContextImpl.context);
            }
            else if constexpr (same_as<T, RFuncReturn_Set>)
            {
                return GetType(ret.type, lContextImpl);
            }
            else if constexpr (same_as<T, RFuncReturn_NotSet>)
            {
                assert(false);
                return nullptr;
            }
            else static_assert(false);

        }, rFuncReturn);

        vector<llvm::Type*> lParamTypes;
        for (auto& rFuncParam : qFuncBody.nFuncDecl->GetUnboundFuncParams())
        {
            auto* lParamType = GetType(rFuncParam.type, lContextImpl);
            lParamTypes.push_back(lParamType);
        }


        std::string name = RNameToString2(qFuncBody.nFuncDecl->GetNDecl()->GetRDecl()->GetIdentifier().name);

        llvm::FunctionType* lFuncType = llvm::FunctionType::get(lRetType, lParamTypes, /*isVarArg*/false);
        auto *lFunc = llvm::Function::Create(lFuncType, llvm::GlobalValue::LinkageTypes::ExternalLinkage, name, _module);

        auto* prelude = llvm::BasicBlock::Create(lContextImpl.context, "prelude");
        llvm::IRBuilder preludeBuilder{prelude};

        vector<llvm::Value*> slotValues;
        slotValues.reserve(qFuncBody.slotInfos.size());

        // 함수 인자 이름 붙이기.
        for (auto& qSlotInfo : qFuncBody.slotInfos)
        {
            // 함수 초기에 할당

            if (qSlotInfo.oArgIndex) // 아규먼트에 매핑 되어있으면,
            {
                auto* lArg = lFunc->getArg(*qSlotInfo.oArgIndex);
                lArg->setName(qSlotInfo.name);
                slotValues.push_back(lArg);

                auto* lType = GetType(qSlotInfo.qType, lContextImpl);
                auto* allocaInst = preludeBuilder.CreateAlloca(lType, nullptr, qSlotInfo.name + "_ptr");

                preludeBuilder.CreateStore(lArg, allocaInst);
                slotValues.push_back(allocaInst);
            }
            else
            {
                // 함수 초기에 할당
                auto* lType = GetType(qSlotInfo.qType, lContextImpl);
                auto* allocaInst = preludeBuilder.CreateAlloca(lType, nullptr, qSlotInfo.name);
                slotValues.push_back(allocaInst);
            }
        }

        // basic block을 미리 다 만들어 두는게 나을거 같다
        vector<llvm::BasicBlock*> lBlocks;
        for (auto* qBlock : qFuncBody.blocks)
        {
            auto* lBlock = llvm::BasicBlock::Create(lContextImpl.context, qBlock->GetName(), lFunc);
            lBlocks.push_back(lBlock);
        }

        for (size_t i = 0, count = qFuncBody.blocks.size(); i < count; i++)
        {
            auto* qBlock = qFuncBody.blocks[i];
            auto* lBlock = lBlocks[i];

            llvm::IRBuilder builder{lBlock};
            for (auto& qInst : qBlock->GetInsts())
            {
                visit([&builder, &lContextImpl, &slotValues, &qFuncBody, &lBlocks](auto& qInst)
                {
                    using T = remove_cvref_t<decltype(qInst)>;
                    if constexpr (same_as<T, QInst_InitString>)
                    {
                        // dest slot에 string이 들어가 있다. 런타임 함수 호출
                        builder.CreateCall();

                        throw NotImplementedException{};
                    }
                    else if constexpr (same_as<T, QInst_Load>)
                    {
                        auto* lPtrValueType = llvm::PointerType::get(lContextImpl.context, 0);
                        auto* lPtrValue = builder.CreateLoad(lPtrValueType, slotValues[qInst.src.index]);
                        auto* lValueType = GetType(qInst.type, lContextImpl);
                        auto* lValue = builder.CreateLoad(lValueType, lPtrValue);

                        // 1:1변환이므로, 다시 slot에 저장한다
                        builder.CreateStore(lValue, slotValues[qInst.dest.index]);
                    }
                    else if constexpr (same_as<T, QInst_Store>) 
                    {
                        // source가 뭐냐에따라 달라진다
                        auto* lValue = GetValue(qInst.src, qInst.type, slotValues, lContextImpl, builder);

                        auto* lPtrValueType = llvm::PointerType::get(lContextImpl.context, 0);
                        auto* lPtrValue = builder.CreateLoad(lPtrValueType, slotValues[qInst.dest.index]);
                        builder.CreateStore(lValue, lPtrValue);
                    }
                    else if constexpr (same_as<T, QInst_AddrOf>) 
                    { 
                        auto* lPtrValueType = llvm::PointerType::get(lContextImpl.context, 0);
                        auto* lPtrValue = builder.CreateLoad(lPtrValueType, slotValues[qInst.dest.index]);
                        builder.CreateStore(slotValues[qInst.slot.index], lPtrValue);
                    }
                    else if constexpr (same_as<T, QInst_Assign>)
                    {
                        // source가 뭐냐에따라 달라진다
                        auto* lValue = GetValue(qInst.src, qInst.type, slotValues, lContextImpl, builder);
                        builder.CreateStore(lValue, slotValues[qInst.dest.index]);
                    }
                    else if constexpr (same_as<T, QInst_Call>)
                    {
                        throw NotImplementedException{};
                    }
                    else if constexpr (same_as<T, QInst_Return>) { throw NotImplementedException{}; }
                    else if constexpr (same_as<T, QInst_Intrinsic>) 
                    { 
                        throw NotImplementedException{};
                    }
                    else if constexpr (same_as<T, QInst_CondJump>) 
                    {
                        auto* lCond = builder.CreateLoad(llvm::Type::getInt1Ty(lContextImpl.context), slotValues[qInst.cond.index]);
                        builder.CreateCondBr(lCond, lBlocks[qInst.trueBlock->GetIndex()], lBlocks[qInst.falseBlock->GetIndex()]);
                    }
                    else if constexpr (same_as<T, QInst_Jump>) 
                    { 
                        builder.CreateBr(lBlocks[qInst.block->GetIndex()]);
                    }
                    else static_assert(false);
                }, qInst);
            }
        }
    }

    assert(false);
    return nullptr;
}

} // namespace Citron