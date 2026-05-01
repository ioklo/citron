#include "MCreateToQInsts.h"
#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RFuncDecl.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MInitExp.h"
#include "MIR/MCallable.h"
#include "QIR/QInsts.h"
#include "MLocToQInsts.h"
#include "MStmtToQInsts.h"
#include "MqBodyContext.h"
#include "MqTranslationContexts.h"
#include "MReadToQInsts.h"
#include "CommonQInstsTranslation.h"
#include "MqEmitState.h"
#include "MqAbi.h"
#include "MqFuncInfo.h"
#include "MqIntrinsicInfo.h"
#include "MqFactory.h"

using namespace std;

namespace Citron {

struct MCreate_BCQInstsTranslator
{
    using ResultType = expected<MqEmitState<void>, DiagPtr>;
    MqCreateTarget createTarget;
    MqTranslationContexts& contexts;

    ResultType Visit(MExp* exp)
    {
        throw NotImplementedException{};
    }

    // load(loc)
    ResultType Visit(MExp_Load* exp)
    {
        // 이 translation으로 lv(slot/ptr)가 하나 나올 것이다
        auto e_s_srcLoc = TranslateMLocToQInsts(exp->loc, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_srcLoc);

        visit([this, exp](auto& srcLoc) {
            using T = remove_cvref_t<decltype(srcLoc)>;
            if constexpr (same_as<T, QLocResult_Slot>)
            {
                auto* type = GetType(exp->loc, &*contexts.rFactory);
                UpdateCreateTarget_Value(type, srcLoc.slotIndex, createTarget, contexts);
            }
            else if constexpr (same_as<T, QLocResult_Ptr>)
            {   
                auto* type = GetType(exp->loc, &*contexts.rFactory);
                UpdateCreateTarget_Ptr(type, srcLoc.slotIndex, createTarget, contexts);
            }
            else static_assert(false);
        }, **e_s_srcLoc);

        return MqEmitState_Ready{};
    }

    // MExp_Store(MLoc* dest, MRead src)
    // *dest = src
    ResultType Visit(MExp_Store* exp)
    {
        // dest를 먼저 계산한다
        auto e_s_dest = TranslateMLocToQInsts(exp->dest, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_dest);

        // src를 translation 하면, slot 또는 ptr이 나올 것이다
        // exp->src가 MRead_Exp라고 하더라도, 별도로 exp로 계산해서 dest에 바로 넣지 않도록 한다
        // a = F(a) 꼴이 나오면, F(a)를 완전히 다 계산하고 난 뒤에 a에 넣는게 안전하다
        auto e_s_src = TranslateMReadToQInsts(exp->src, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_src);

        return visit([this](auto& destLoc, auto& src) -> ResultType {
            using T = remove_cvref_t<decltype(destLoc)>;
            using U = remove_cvref_t<decltype(src)>;

            // 1-1. destLoc: slot, srcLoc: slot
            if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_Slot>)
            {
                // <destSlot> = <srcSlot>
                auto* type = contexts.bodyContext.GetSlotType(src.slotIndex);
                contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Dest_Slot{destLoc.slotIndex}, QArg_Value_Slot{src.slotIndex}});

                UpdateCreateTarget_Value(type, destLoc.slotIndex, createTarget, contexts);
                return MqEmitState_Ready{};
            }
            // 1-2. destLoc: slot, srcLoc: ptr
            else if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_Ptr>)
            {
                // dest = *src
                auto* type = contexts.bodyContext.GetSlotType(destLoc.slotIndex);
                contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Dest_Slot{destLoc.slotIndex}, QArg_Addr_PtrSlot{src.slotIndex}});

                UpdateCreateTarget_Value(type, destLoc.slotIndex, createTarget, contexts);
                return MqEmitState_Ready{};
            }
            // 1-3. destLoc: slot, srcLoc: const bool
            else if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_ConstBool>)
            {
                auto* boolType = contexts.bodyContext.GetBoolType();

                // dest = const
                contexts.bodyContext.EmitInst(QInst_Assign{boolType, QArg_Dest_Slot{destLoc.slotIndex}, QArg_Value_ConstBool{src.value}});
                UpdateCreateTarget(boolType, QArg_Value_ConstBool{src.value}, createTarget, contexts);
                return MqEmitState_Ready{};
            }

            // 1-4. destLoc: slot, srcLoc: const int
            else if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_ConstInt32>)
            {
                auto* intType = contexts.bodyContext.GetIntType();

                // dest = const
                contexts.bodyContext.EmitInst(QInst_Assign{intType, QArg_Dest_Slot{destLoc.slotIndex}, QArg_Value_ConstInt32{src.value}});
                UpdateCreateTarget(intType, QArg_Value_ConstInt32{src.value}, createTarget, contexts);
                return MqEmitState_Ready{};
            }

            // 2-1. destLoc: ptr, srcLoc: slot
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_Slot>)
            {
                // *dest = src
                auto* type = contexts.bodyContext.GetSlotType(src.slotIndex);
                contexts.bodyContext.EmitInst(QInst_Store{type, QArg_Addr_PtrSlot{destLoc.slotIndex}, QArg_Value_Slot{src.slotIndex}});
                UpdateCreateTarget_Value(type, src.slotIndex, createTarget, contexts);
                return MqEmitState_Ready{};
            }
            // 2-2. destLoc: ptr, srcLoc: ptr
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_Ptr>)
            {   
                auto* type = contexts.bodyContext.GetSlotType(destLoc.slotIndex);
                size_t size = contexts.qAbi->GetTypeSize(type);
                contexts.bodyContext.EmitIntrinsic(
                    QInst_IntrinsicKind::Memcpy_Void_Ptr_Ptr_Int,
                    /*o_dest*/nullopt,
                    {QArg_CallArg_Slot{destLoc.slotIndex}, QArg_CallArg_Slot{src.slotIndex}, QArg_CallArg_ConstInt32{(int)size}});

                UpdateCreateTarget_Ptr(type, src.slotIndex, createTarget, contexts);
                return MqEmitState_Ready{};
            }

            // 2-3. destLoc: ptr, srcLoc: const bool
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_ConstBool>)
            {
                auto* boolType = contexts.bodyContext.GetBoolType();

                // dest = const
                contexts.bodyContext.EmitInst(QInst_Store{boolType, QArg_Addr_PtrSlot{destLoc.slotIndex}, QArg_Value_ConstBool{src.value}});
                UpdateCreateTarget(boolType, QArg_Value_ConstBool{src.value}, createTarget, contexts);
                return MqEmitState_Ready{};
            }

            // 2-4. destLoc: ptr, srcLoc: const int
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_ConstInt32>)
            {
                auto* intType = contexts.bodyContext.GetIntType();

                // dest = const
                contexts.bodyContext.EmitInst(QInst_Store{intType, QArg_Addr_PtrSlot{destLoc.slotIndex}, QArg_Value_ConstInt32{src.value}});
                UpdateCreateTarget(intType, QArg_Value_ConstInt32{src.value}, createTarget, contexts);
                return MqEmitState_Ready{};
            }
            else static_assert(false);

        }, **e_s_dest, **e_s_src);
    }

    ResultType Visit(MExp_Stmt* exp) 
    {
        auto e_s_result = TranslateMStmtsToQInsts(exp->stmts, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_result);

        // exp->finalExp는 context dependent하다. 여기는 MCreate_BC를 Translation하기 위한 곳이므로, finalExp를 MCreate로 간주하고 Translate한다
        return Accept(MCreate_BCQInstsTranslator{createTarget, contexts}, exp->finalExp);
    }

    // &x
    ResultType Visit(MExp_PtrRef* exp)
    {
        auto e_s_loc = TranslateMLocToQInsts(exp->innerLoc, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_loc);

        visit([this](auto& loc) {
            using T = remove_cvref_t<decltype(loc)>;

            if constexpr (same_as<T, QLocResult_Slot>)
            {   
                UpdateCreateTarget_AddrOf(loc.slotIndex, createTarget, contexts);
            }
            else if constexpr (same_as<T, QLocResult_Ptr>)
            {
                auto* ptrType = contexts.bodyContext.GetSlotType(loc.slotIndex);
                UpdateCreateTarget_Value(ptrType, loc.slotIndex, createTarget, contexts);
            }
            else static_assert(false);
        }, **e_s_loc);

        return MqEmitState_Ready{};
    }

    ResultType Visit(MExp_BoolLiteral* exp) 
    {
        UpdateCreateTarget(contexts.bodyContext.GetBoolType(), QArg_Value_ConstBool{exp->value}, createTarget, contexts);
        return MqEmitState_Ready{};
    }

    ResultType Visit(MExp_IntLiteral* exp) 
    { 
        UpdateCreateTarget(contexts.bodyContext.GetIntType(), QArg_Value_ConstInt32{exp->value}, createTarget, contexts);
        return MqEmitState_Ready{};
    }

    ResultType Visit(MExp_CallIntrinsic* exp) 
    {
        auto& intrinsicInfo = contexts.mqFactory->GetIntrinsicInfo(exp->kind);
        auto e_s_o_retLocResult = HandleIntrinsicCall(intrinsicInfo, createTarget, exp->typeArgs, exp->args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_o_retLocResult);
        return MqEmitState_Ready{};
    }

    ResultType Visit(MExp_Call* exp) 
    { 
        auto e_s_o_retLocResult = HandleCall(exp->callable.decl, exp->callable.typeArgs, createTarget, exp->callable.o_instance, exp->args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_o_retLocResult);

        return MqEmitState_Ready{};
    }

    // BC용 Struct
    ResultType Visit(MExp_NewStruct* exp) 
    { 
        // TODO: [33] struct [BitwiseCopyable] 추가
        throw NotImplementedException{};
    }

    ResultType Visit(MExp_NewEnumElem* exp) 
    {
        // TODO: [34] enum [BitwiseCopyable] 지원
        throw NotImplementedException{};
    }

    // ResultType Visit(MExp_Nullable* exp) { }
    // ResultType Visit(MExp_NullableNullLiteral* exp) { }
    // ResultType Visit(MExp_Cast* exp) { }
    // ResultType Visit(MExp_Lambda* exp) { }
    ResultType Visit(MExp_InlineBlock* exp) 
    {
        return HandleInlineBlock(exp->scope, createTarget, contexts);
    }
    // ResultType Visit(MExp_Is* exp) {}
};

struct MCreate_NBCQInstsTranslator
{
    using ResultType = expected<MqEmitState<void>, DiagPtr>;
    MqCreateTarget createTarget;
    MqTranslationContexts& contexts;

    ResultType Visit(MInitExp* initExp)
    {
        throw NotImplementedException{};
    }

    // ResultType Visit(MInitExp_Shared* mInitExp) { }
    // ResultType Visit(MInitExp_SharedRef* mInitExp) { }
    // ResultType Visit(MInitExp_Stmt* mInitExp) { }
    ResultType Visit(MInitExp_String* mInitExp) 
    { 
        return TranslateMInitExp_StringToQInsts(mInitExp, createTarget, contexts);
    }
    // ResultType Visit(MInitExp_List* mInitExp) { }
    ResultType Visit(MInitExp_CallIntrinsic* mInitExp) 
    { 
        auto& intrinsicInfo = contexts.mqFactory->GetIntrinsicInfo(mInitExp->kind);
        auto e_s_o_retLocResult = HandleIntrinsicCall(intrinsicInfo, createTarget, mInitExp->typeArgs, mInitExp->args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_o_retLocResult);
        return MqEmitState_Ready{};
    }
    // ResultType Visit(MInitExp_NewClass* mInitExp) { }
    ResultType Visit(MInitExp_StructCtor* mInitExp) 
    {
        // TODO: [61] 일반적인 struct ctor, dtor, copy/move ctor, copy/move assign 구현
        auto* type = GetType(mInitExp, &*contexts.rFactory);
        if (type != contexts.bodyContext.GetStringType())
            throw NotImplementedException{};

        return visit([this](auto& kind) -> ResultType {
            using U = remove_cvref_t<decltype(kind)>;
            if constexpr (same_as<U, MInitExp_StructCtorKind_Copy>)
            {
                auto e_s_srcResult = TranslateMLocToQInsts(kind.src.loc, contexts);
                RETURN_ON_ERROR_OR_DONE(e_s_srcResult);

                if (auto o_createTargetArg = MakeAddrCallArg(createTarget, contexts))
                {
                    auto srcArg = MakeAddrCallArg(**e_s_srcResult, contexts);
                    contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::CopyCtor_Void_StringRef_StringInRef, nullopt, 
                        {*o_createTargetArg, srcArg});
                }

                return MqEmitState_Ready{};
            }
            else if constexpr (same_as<U, MInitExp_StructCtorKind_Move>)
            {
                // TODO: [30] move구현
                throw NotImplementedException{};
            }
            else if constexpr (same_as<U, MInitExp_StructCtorKind_General>)
            {
                // TODO: [61] 일반적인 struct ctor, dtor, copy/move ctor, copy/move assign 구현
                throw NotImplementedException{};
            }
            else static_assert(false);
        }, mInitExp->kind);
    }

    ResultType Visit(MInitExp_Call* mInitExp)
    { 
        auto e_s_o_retLocResult = HandleCall(mInitExp->callable.decl, mInitExp->callable.typeArgs, createTarget, mInitExp->callable.o_instance, mInitExp->args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_o_retLocResult);

        return MqEmitState_Ready{};
    }

    // ResultType Visit(MInitExp_NewEnumElem* mInitExp) { }
    // ResultType Visit(MInitExp_Nullable* mInitExp) { }
    // ResultType Visit(MInitExp_NullableNullLiteral* mInitExp) { }
    // ResultType Visit(MInitExp_NullableInplaceNullLiteral* mInitExp) { }
    // ResultType Visit(MInitExp_Cast* mInitExp) { }
    // ResultType Visit(MInitExp_Lambda* mInitExp) { }
    ResultType Visit(MInitExp_InlineBlock* mInitExp) 
    { 
        return HandleInlineBlock(mInitExp->scope, createTarget, contexts);
    }
    // ResultType Visit(MInitExp_As* mInitExp) { }
};

expected<MqEmitState<void>, DiagPtr> TranslateMCreate_NBCToQInsts(MInitExp* mInitExp, MqCreateTarget createTarget, MqTranslationContexts& contexts)
{
    return Accept(MCreate_NBCQInstsTranslator{createTarget, contexts}, mInitExp);
}

expected<MqEmitState<void>, DiagPtr> TranslateMCreateToQInsts(MCreate& mCreate, MqCreateTarget createTarget, MqTranslationContexts& contexts)
{
    return visit([createTarget, &contexts](auto& mCreate) -> expected<MqEmitState<void>, DiagPtr> {
        using T = remove_cvref_t<decltype(mCreate)>;

        if constexpr (same_as<T, MCreate_BC>)
        {
            return Accept(MCreate_BCQInstsTranslator{createTarget, contexts}, mCreate.exp);
        }
        else if constexpr (same_as<T, MCreate_NBC>)
        {
            return Accept(MCreate_NBCQInstsTranslator{createTarget, contexts}, mCreate.initExp);
        }
        else static_assert(false);
        
    }, mCreate);
}

} // namespace Citron