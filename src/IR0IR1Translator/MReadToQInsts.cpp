#include "MReadToQInsts.h"

#include <optional>
#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "MIR/MExp.h"
#include "MIR/MRead.h"
#include "MIR/MLoc.h"
#include "MLocToQInsts.h"
#include "MCreateToQInsts.h"
#include "MStmtToQInsts.h"
#include "QBodyContext.h"
#include "QTranslationContexts.h"
#include "QEmitState.h"
#include "CommonQInstsTranslation.h"
#include "QEmitState.h"
#include "MqIntrinsicInfo.h"
#include "MqFactory.h"

using namespace std;

namespace Citron {

namespace {
QReadResult ToReadResult(QLocResult& locResult)
{
    return visit([](auto& loc) -> QReadResult {
        using T = remove_cvref_t<decltype(loc)>;
        if constexpr (same_as<T, QLocResult_Slot>) return QReadResult_Slot{loc.slotIndex};
        else if constexpr (same_as<T, QLocResult_Ptr>) return QReadResult_Ptr{loc.slotIndex};
        else static_assert(false);
    }, locResult);
}
} // namespace

// slot을 하나 만들어서 리턴한다
struct MRead_ExpQInstsTranslator
{
    using ResultType = expected<QEmitState<QReadResult>, DiagPtr>;
    QTranslationContexts& contexts;

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

        // 그냥 변환기
        return ToReadResult(**e_s_srcLoc);

        /*return visit([this, exp](auto& srcLoc) -> ResultType
        {
            using T = remove_cvref_t<decltype(srcLoc)>;
            if constexpr (same_as<T, QLocResult_Slot>)
            {
                auto* type = GetType(exp, &*contexts.rFactory);
                size_t destSlotIndex = contexts.bodyContext.AddTemp(type, "load"); 
                contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Dest_Slot{destSlotIndex}, QArg_Value_Slot{srcLoc.slotIndex}});

                return QReadResult_Slot{destSlotIndex};
            }
            else if constexpr (same_as<T, QLocResult_Ptr>)
            {
                auto* type = GetType(exp, &*contexts.rFactory);
                size_t destSlotIndex = contexts.bodyContext.AddTemp(type, "load");
                contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Dest_Slot{destSlotIndex}, QArg_Addr_PtrSlot{srcLoc.slotIndex}});

                return QReadResult_Slot{destSlotIndex};
            }
            else static_assert(false);

        }, **e_s_srcLoc);*/
    }

    // Assign(loc dest, src exp), Store
    // *loc = exp;
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

                return QReadResult_Slot{destLoc.slotIndex};
            }
            // 1-2. destLoc: slot, srcLoc: ptr
            else if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_Ptr>)
            {
                // dest = *src
                auto* type = contexts.bodyContext.GetSlotType(destLoc.slotIndex);
                contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Dest_Slot{destLoc.slotIndex}, QArg_Addr_PtrSlot{src.slotIndex}});

                return QReadResult_Slot{destLoc.slotIndex};
            }
            // 1-3. destLoc: slot, srcLoc: const bool
            else if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_ConstBool>)
            {
                // dest = const
                auto* boolType = contexts.bodyContext.GetBoolType();
                contexts.bodyContext.EmitInst(QInst_Assign{boolType, QArg_Dest_Slot{destLoc.slotIndex}, QArg_Value_ConstBool{src.value}});
                return src; // const 그대로 리턴
            }
            // 1-4. destLoc: slot, srcLoc: const int32
            else if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_ConstInt32>)
            {
                // dest = const
                auto* intType = contexts.bodyContext.GetIntType();
                contexts.bodyContext.EmitInst(QInst_Assign{intType, QArg_Dest_Slot{destLoc.slotIndex}, QArg_Value_ConstInt32{src.value}});
                return src; // const 그대로 리턴
            }

            // 2-1. destLoc: ptr, srcLoc: slot
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_Slot>)
            {
                // *dest = src
                auto* type = contexts.bodyContext.GetSlotType(src.slotIndex);
                contexts.bodyContext.EmitInst(QInst_Store{type, QArg_Addr_PtrSlot{destLoc.slotIndex}, QArg_Value_Slot{src.slotIndex}});

                return QReadResult_Slot{src.slotIndex}; // src slot을 그대로 리턴한다
            }
            // 2-2. destLoc: ptr, srcLoc: ptr
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_Ptr>)
            {
                auto* type = contexts.bodyContext.GetSlotType(destLoc.slotIndex);
                size_t resultSlotIndex = contexts.bodyContext.AddTemp(type, "load");

                // result = *src                            
                contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Dest_Slot{resultSlotIndex}, QArg_Addr_PtrSlot{src.slotIndex}});

                // *dest = result
                contexts.bodyContext.EmitInst(QInst_Store{type, QArg_Addr_PtrSlot{destLoc.slotIndex}, QArg_Value_Slot{resultSlotIndex}});

                return QReadResult_Slot{resultSlotIndex}; // src에서 로드한 값을 리턴한다
            }
            // 2-3. destLoc: ptr, srcLoc: const bool
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_ConstBool>)
            {
                // dest = const
                auto* boolType = contexts.bodyContext.GetBoolType();
                contexts.bodyContext.EmitInst(QInst_Store{boolType, QArg_Addr_PtrSlot{destLoc.slotIndex}, QArg_Value_ConstBool{src.value}});
                return src; // const 그대로 리턴
            }
            // 2-4. destLoc: ptr, srcLoc: const int32
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_ConstInt32>)
            {
                // dest = const
                auto* intType = contexts.bodyContext.GetIntType();
                contexts.bodyContext.EmitInst(QInst_Store{intType, QArg_Addr_PtrSlot{destLoc.slotIndex}, QArg_Value_ConstInt32{src.value}});
                return src; // const 그대로 리턴
            }
            else static_assert(false);

        }, **e_s_dest, **e_s_src);
    }

    ResultType Visit(MExp_Stmt* exp)
    {
        auto e_s_result = TranslateMStmtsToQInsts(exp->stmts, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_result);

        // exp->finalExp는 context dependent하다. 여기는 MRead_Exp를 Translation하기 위한 곳이므로, finalExp를 MRead로 간주하고 Translate한다
        return Accept(MRead_ExpQInstsTranslator{contexts}, exp->finalExp);
    }

    // &x
    ResultType Visit(MExp_PtrRef* exp)
    {
        auto e_s_loc = TranslateMLocToQInsts(exp->innerLoc, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_loc);

        return visit([this](auto& loc) -> ResultType {
            using T = remove_cvref_t<decltype(loc)>;

            if constexpr (same_as<T, QLocResult_Slot>)
            {
                auto* ptrType = contexts.bodyContext.GetPtrType();
                size_t resultSlotIndex = contexts.bodyContext.AddTemp(ptrType, "ptr_ref");

                // slot의 addrof를 하나 한다 ptr 타입
                contexts.bodyContext.EmitInst(QInst_AddrOf{QArg_Dest_Slot{resultSlotIndex}, loc.slotIndex});

                return QReadResult_Slot{resultSlotIndex};
            }
            else if constexpr (same_as<T, QLocResult_Ptr>)
            {
                auto* ptrType = contexts.bodyContext.GetPtrType();
                size_t resultSlotIndex = contexts.bodyContext.AddTemp(ptrType, "ptr_ref");

                // ptr이면, resultSlotIndex에 복사해서 넣어준다. QReadResult_Ptr로 직접 전해주지 않도록 한다 (value로만 전달)
                contexts.bodyContext.EmitInst(QInst_Assign{ptrType, QArg_Dest_Slot{resultSlotIndex}, QArg_Value_Slot{loc.slotIndex}});

                return QReadResult_Slot{resultSlotIndex};
            }
            else static_assert(false);

        }, **e_s_loc);
    }

    ResultType Visit(MExp_BoolLiteral* exp) 
    {
        return QReadResult_ConstBool{exp->value};
    }

    ResultType Visit(MExp_IntLiteral* exp) 
    {
        return QReadResult_ConstInt32{exp->value};
    }

    ResultType Visit(MExp_CallIntrinsic* exp) 
    {
        auto& intrinsicInfo = contexts.mqFactory->GetIntrinsicInfo(exp->kind);
        auto e_s_o_retLocResult = HandleIntrinsicCall(intrinsicInfo, MqCreateTarget_Discard{}, exp->typeArgs, exp->args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_o_retLocResult);

        assert(**e_s_o_retLocResult); // void가 아닌 intrinsic이므로, 항상 slot이 나와야 한다
        return ToReadResult(***e_s_o_retLocResult);
    }
    
    ResultType Visit(MExp_Call* exp) 
    {
        auto e_s_o_retLocResult = HandleCall(exp->callable.decl, exp->callable.typeArgs, MqCreateTarget_Discard{}, exp->callable.o_instance, exp->args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_o_retLocResult);

        assert(**e_s_o_retLocResult); // void가 아닌 함수이므로, 항상 slot이 나와야 한다
        return ToReadResult(***e_s_o_retLocResult);
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
        size_t leaveSlotIndex = contexts.bodyContext.AddTemp(exp->returnType, "inline_block");
        auto e_s_result = HandleInlineBlock(exp->scope, MqCreateTarget_Slot{leaveSlotIndex}, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_result);

        return QReadResult_Slot{leaveSlotIndex};
    }
    // ResultType Visit(MExp_Is* exp) {}
   
};

expected<QEmitState<QReadResult>, DiagPtr> TranslateMRead_LocToQInsts(MRead_Loc& mReadLoc, QTranslationContexts& contexts)
{
    // 이건 MLoc을 그대로 써본다
    auto e_s_locResult = TranslateMLocToQInsts(mReadLoc.loc, contexts);
    RETURN_ON_ERROR_OR_DONE(e_s_locResult);

    return ToReadResult(**e_s_locResult);
}

expected<QEmitState<QReadResult>, DiagPtr> TranslateMRead_ExpToQInsts(MRead_Exp& mReadExp, QTranslationContexts& contexts)
{
    return Accept(MRead_ExpQInstsTranslator{contexts}, mReadExp.exp);
}

expected<QEmitState<QReadResult>, DiagPtr> TranslateMReadToQInsts(MRead& mRead, QTranslationContexts& contexts)
{
    return visit([&contexts](auto& mRead) -> expected<QEmitState<QReadResult>, DiagPtr> {
        using T = remove_cvref_t<decltype(mRead)>;
        if constexpr (same_as<T, MRead_Loc>)
        {
            auto e_s_result = TranslateMRead_LocToQInsts(mRead, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_result);

            return **e_s_result;
        }
        else if constexpr (same_as<T, MRead_Exp>)
        {
            auto e_s_result = TranslateMRead_ExpToQInsts(mRead, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_result);

            return **e_s_result;
        }
        else static_assert(false);
    }, mRead);

}

} // namespace Citron