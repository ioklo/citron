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
#include "CommonQInstsTranslation.h"

using namespace std;

namespace Citron {

// slot을 하나 만들어서 리턴한다
struct MRead_ExpQInstsTranslator
{
    using ResultType = expected<QReadResult_Value, DiagPtr>;
    QTranslationContexts& contexts;

    ResultType Visit(MExp* exp)
    {
        throw NotImplementedException{};
    }

    // load(loc)
    ResultType Visit(MExp_Load* exp)
    {
        // 이 translation으로 lv(slot/ptr)가 하나 나올 것이다
        auto e_srcLoc = TranslateMLocToQInsts(exp->loc, contexts);
        RETURN_ON_ERROR(e_srcLoc);

        return visit([this, exp](auto& srcLoc) -> ResultType
        {
            using T = remove_cvref_t<decltype(srcLoc)>;
            if constexpr (same_as<T, QLocResult_Slot>)
            {
                auto* type = GetType(exp, &*contexts.rFactory);
                size_t destSlotIndex = contexts.bodyContext.NewSlot(type); 
                auto e_result = contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Slot{destSlotIndex}, QArg_Slot{srcLoc.slotIndex}});
                RETURN_ON_ERROR(e_result);

                return QReadResult_Slot{destSlotIndex};
            }
            else if constexpr (same_as<T, QLocResult_Ptr>)
            {
                auto* type = GetType(exp, &*contexts.rFactory);
                size_t destSlotIndex = contexts.bodyContext.NewSlot(type);
                auto e_result = contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Slot{destSlotIndex}, QArg_Slot{srcLoc.slotIndex}});
                RETURN_ON_ERROR(e_result);

                return QReadResult_Slot{destSlotIndex};
            }
            else static_assert(false);

        }, *e_srcLoc);
    }

    // Assign(loc dest, src exp), Store
    // *loc = exp;
    ResultType Visit(MExp_Store* exp)
    {
        // dest를 먼저 계산한다
        auto e_dest = TranslateMLocToQInsts(exp->dest, contexts);
        RETURN_ON_ERROR(e_dest);

        // src를 translation 하면, slot 또는 ptr이 나올 것이다
        // exp->src가 MRead_Exp라고 하더라도, 별도로 exp로 계산해서 dest에 바로 넣지 않도록 한다
        // a = F(a) 꼴이 나오면, F(a)를 완전히 다 계산하고 난 뒤에 a에 넣는게 안전하다
        auto e_src = TranslateMReadToQInsts(exp->src, contexts);
        RETURN_ON_ERROR(e_src);

        return visit([this](auto& destLoc, auto& src) -> ResultType {
            using T = remove_cvref_t<decltype(destLoc)>;
            using U = remove_cvref_t<decltype(src)>;

            // 1-1. destLoc: slot, srcLoc: slot
            if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_Slot>)
            {
                // <destSlot> = <srcSlot>
                auto* type = contexts.bodyContext.GetSlotType(src.slotIndex);
                auto e_emitResult = contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Slot{destLoc.slotIndex}, QArg_Slot{src.slotIndex}});
                RETURN_ON_ERROR(e_emitResult);

                return QReadResult_Slot{destLoc.slotIndex};
            }
            // 1-2. destLoc: slot, srcLoc: ptr
            else if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_Ptr>)
            {
                // dest = *src
                auto* type = contexts.bodyContext.GetSlotType(destLoc.slotIndex);
                auto e_emitResult = contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Slot{destLoc.slotIndex}, QArg_Slot{src.slotIndex}});
                RETURN_ON_ERROR(e_emitResult);

                return QReadResult_Slot{destLoc.slotIndex};
            }
            // 1-3. destLoc: slot, srcLoc: const bool
            else if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_ConstBool>)
            {
                // dest = const
                auto* boolType = contexts.bodyContext.GetBoolType();
                auto e_emitResult = contexts.bodyContext.EmitInst(QInst_Assign{boolType, QArg_Slot{destLoc.slotIndex}, QArg_ConstBool{src.value}});
                RETURN_ON_ERROR(e_emitResult);
                return src; // const 그대로 리턴
            }
            // 1-4. destLoc: slot, srcLoc: const int32
            else if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_ConstInt32>)
            {
                // dest = const
                auto* intType = contexts.bodyContext.GetIntType();
                auto e_emitResult = contexts.bodyContext.EmitInst(QInst_Assign{intType, QArg_Slot{destLoc.slotIndex}, QArg_ConstInt32{src.value}});
                RETURN_ON_ERROR(e_emitResult);
                return src; // const 그대로 리턴
            }

            // 2-1. destLoc: ptr, srcLoc: slot
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_Slot>)
            {
                // *dest = src
                auto* type = contexts.bodyContext.GetSlotType(src.slotIndex);
                auto e_emitResult = contexts.bodyContext.EmitInst(QInst_Store{type, QArg_Slot{destLoc.slotIndex}, QArg_Slot{src.slotIndex}});
                RETURN_ON_ERROR(e_emitResult);

                return QReadResult_Slot{src.slotIndex}; // src slot을 그대로 리턴한다
            }
            // 2-2. destLoc: ptr, srcLoc: ptr
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_Ptr>)
            {
                auto* type = contexts.bodyContext.GetSlotType(destLoc.slotIndex);
                size_t resultSlotIndex = contexts.bodyContext.NewSlot(type);

                // result = *src                            
                auto e_emitResult = contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Slot{resultSlotIndex}, QArg_Slot{src.slotIndex}});
                RETURN_ON_ERROR(e_emitResult);

                // *dest = result
                e_emitResult = contexts.bodyContext.EmitInst(QInst_Store{type, QArg_Slot{destLoc.slotIndex}, QArg_Slot{resultSlotIndex}});
                RETURN_ON_ERROR(e_emitResult);

                return QReadResult_Slot{resultSlotIndex}; // src에서 로드한 값을 리턴한다
            }
            // 2-3. destLoc: ptr, srcLoc: const bool
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_ConstBool>)
            {
                // dest = const
                auto* boolType = contexts.bodyContext.GetBoolType();
                auto e_emitResult = contexts.bodyContext.EmitInst(QInst_Store{boolType, QArg_Slot{destLoc.slotIndex}, QArg_ConstBool{src.value}});
                RETURN_ON_ERROR(e_emitResult);
                return src; // const 그대로 리턴
            }
            // 2-4. destLoc: ptr, srcLoc: const int32
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_ConstInt32>)
            {
                // dest = const
                auto* intType = contexts.bodyContext.GetIntType();
                auto e_emitResult = contexts.bodyContext.EmitInst(QInst_Store{intType, QArg_Slot{destLoc.slotIndex}, QArg_ConstInt32{src.value}});
                RETURN_ON_ERROR(e_emitResult);
                return src; // const 그대로 리턴
            }
            else static_assert(false);

        }, *e_dest, *e_src);
    }

    ResultType Visit(MExp_Stmt* exp)
    {
        auto e_result = TranslateMStmtsToQInsts(exp->stmts, contexts);
        RETURN_ON_ERROR(e_result);

        // exp->finalExp는 context dependent하다. 여기는 MRead_Exp를 Translation하기 위한 곳이므로, finalExp를 MRead로 간주하고 Translate한다
        return Accept(MRead_ExpQInstsTranslator{contexts}, exp->finalExp);
    }

    // &x
    ResultType Visit(MExp_PtrRef* exp)
    {
        auto e_loc = TranslateMLocToQInsts(exp->innerLoc, contexts);
        RETURN_ON_ERROR(e_loc);

        return visit([this](auto& loc) -> ResultType {
            using T = remove_cvref_t<decltype(loc)>;

            if constexpr (same_as<T, QLocResult_Slot>)
            {
                auto* ptrType = contexts.bodyContext.GetPtrType();
                size_t resultSlotIndex = contexts.bodyContext.NewSlot(ptrType);

                // slot의 addrof를 하나 한다 ptr 타입
                auto e_addrResult = contexts.bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{resultSlotIndex}, QArg_Slot{loc.slotIndex}});
                RETURN_ON_ERROR(e_addrResult);

                return QReadResult_Slot{resultSlotIndex};
            }
            else if constexpr (same_as<T, QLocResult_Ptr>)
            {
                auto* ptrType = contexts.bodyContext.GetPtrType();
                size_t resultSlotIndex = contexts.bodyContext.NewSlot(ptrType);

                // ptr이면, destSlotIndex에 복사해서 넣어준다. QReadResult_Ptr로 직접 전해주지 않도록 한다 (value로만 전달)
                auto e_emitResult = contexts.bodyContext.EmitInst(QInst_Assign{ptrType, QArg_Slot{resultSlotIndex}, QArg_Slot{loc.slotIndex}});
                RETURN_ON_ERROR(e_emitResult);

                return QReadResult_Slot{resultSlotIndex};
            }
            else static_assert(false);

            return {};
        }, *e_loc);
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
        auto* intrinsicInfo = GetIntrinsicInfo(exp->kind, contexts);
        assert(intrinsicInfo);

        auto e_args = TranslateMArgumentsToQInsts(exp->args, contexts);

        // 여긴 MExp이므로 void type은 들어오지 않는다
        size_t resultSlotIndex = contexts.bodyContext.NewSlot(intrinsicInfo->type);
        auto e_result = contexts.bodyContext.EmitIntrinsic(intrinsicInfo->kind, resultSlotIndex, move(*e_args));
        RETURN_ON_ERROR(e_result);

        return QReadResult_Slot{resultSlotIndex};
    }

    ResultType Visit(MExp_Call* exp) 
    {
        // generics는 어떻게 하나요
        // T F<T>(T t) { return t; }
        // Generics는 T에 관한 정보를 더 넘겨준다 (크기 등)
        // 따라서 이 함수는 t, {F함수에 대한 constraint table} 두 인자를 받는다
        // 그리고 t는 항상 stack pointer를 가리키게 된다 (callee쪽에서 크기를 정확히 알 수 없으므로)
        auto e_args = TranslateMArgumentsToQInsts(exp->args, contexts);
        RETURN_ON_ERROR(e_args);

        // 2. Emit처리
        auto* rFuncDecl = GetRFuncDecl(exp->callable);
        auto* retType = GetType(exp->callable);
        size_t resultSlotIndex = contexts.bodyContext.NewSlot(retType);
        auto e_emitResult = contexts.bodyContext.EmitInst(QInst_Call{rFuncDecl, QArg_Slot{resultSlotIndex}, move(*e_args)});
        RETURN_ON_ERROR(e_emitResult);

        return QReadResult_Slot{resultSlotIndex};
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
    // ResultType Visit(MExp_InlineBlock* exp) { }
    // ResultType Visit(MExp_Is* exp) {}
   
};

expected<QReadResult_Place, DiagPtr> TranslateMRead_LocToQInsts(MRead_Loc& mReadLoc, QTranslationContexts& contexts)
{
    // 이건 MLoc을 그대로 써본다
    auto e_locResult = TranslateMLocToQInsts(mReadLoc.loc, contexts);
    RETURN_ON_ERROR(e_locResult);

    return visit([](auto& locResult) -> QReadResult_Place {
        using T = remove_cvref_t<decltype(locResult)>;
        if constexpr (same_as<T, QLocResult_Slot>) return QReadResult_Slot{locResult.slotIndex};
        else if constexpr (same_as<T, QLocResult_Ptr>) return QReadResult_Ptr{locResult.slotIndex};
        else static_assert(false);
    }, *e_locResult);
}

expected<QReadResult_Value, DiagPtr> TranslateMRead_ExpToQInsts(MRead_Exp& mReadExp, QTranslationContexts& contexts)
{
    return Accept(MRead_ExpQInstsTranslator{contexts}, mReadExp.exp);
}

expected<QReadResult, DiagPtr> TranslateMReadToQInsts(MRead& mRead, QTranslationContexts& contexts)
{
    return visit([&contexts](auto& mRead) -> expected<QReadResult, DiagPtr> {
        using T = remove_cvref_t<decltype(mRead)>;
        if constexpr (same_as<T, MRead_Loc>)
        {
            auto e_result = TranslateMRead_LocToQInsts(mRead, contexts);
            RETURN_ON_ERROR(e_result);

            return visit([](auto& result) -> QReadResult { return result; }, *e_result);
        }
        else if constexpr (same_as<T, MRead_Exp>)
        {
            auto e_result = TranslateMRead_ExpToQInsts(mRead, contexts);
            RETURN_ON_ERROR(e_result);

            return visit([](auto& result) -> QReadResult { return result; }, *e_result);
        }
        else static_assert(false);
    }, mRead);

}

} // namespace Citron