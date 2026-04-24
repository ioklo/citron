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
#include "QBodyContext.h"
#include "QTranslationContexts.h"
#include "MReadToQInsts.h"
#include "CommonQInstsTranslation.h"
#include "QEmitState.h"
#include "QAbi.h"
#include "QFuncInfo.h"
#include "QIntrinsicInfo.h"

using namespace std;

namespace Citron {

struct MCreate_BCQInstsTranslator
{
    using ResultType = expected<QEmitState<void>, DiagPtr>;
    optional<size_t> o_destSlotIndex;
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

        if (!o_destSlotIndex) return QEmitState_Ready{};

        visit([this, exp](auto& srcLoc) {
            using T = remove_cvref_t<decltype(srcLoc)>;
            if constexpr (same_as<T, QLocResult_Slot>)
            {
                auto* type = GetType(exp->loc, &*contexts.rFactory);
                contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Dest{*o_destSlotIndex}, QArg_Value_Slot{srcLoc.slotIndex}});
            }
            else if constexpr (same_as<T, QLocResult_Ptr>)
            {
                auto* type = GetType(exp->loc, &*contexts.rFactory);
                contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Dest{*o_destSlotIndex}, QArg_Addr_PtrSlot{srcLoc.slotIndex}});
            }
            else static_assert(false);
        }, **e_s_srcLoc);

        return QEmitState_Ready{};
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
                contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Dest{destLoc.slotIndex}, QArg_Value_Slot{src.slotIndex}});

                if (o_destSlotIndex)
                {
                    // <result> = <destSlot>
                    contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Dest{*o_destSlotIndex}, QArg_Value_Slot{destLoc.slotIndex}});
                }

                return QEmitState_Ready{};
            }
            // 1-2. destLoc: slot, srcLoc: ptr
            else if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_Ptr>)
            {
                // dest = *src
                auto* type = contexts.bodyContext.GetSlotType(destLoc.slotIndex);
                contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Dest{destLoc.slotIndex}, QArg_Addr_PtrSlot{src.slotIndex}});

                if (o_destSlotIndex)
                {
                    // result = dest
                    contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Dest{*o_destSlotIndex}, QArg_Value_Slot{destLoc.slotIndex}});
                }

                return QEmitState_Ready{};
            }
            // 1-3. destLoc: slot, srcLoc: const bool
            else if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_ConstBool>)
            {
                auto* boolType = contexts.bodyContext.GetBoolType();

                // dest = const
                contexts.bodyContext.EmitInst(QInst_Assign{boolType, QArg_Dest{destLoc.slotIndex}, QArg_Value_ConstBool{src.value}});
                
                if (o_destSlotIndex)
                {
                    // result = dest
                    contexts.bodyContext.EmitInst(QInst_Assign{boolType, QArg_Dest{*o_destSlotIndex}, QArg_Value_ConstBool{src.value}});
                }

                return QEmitState_Ready{};
            }

            // 1-4. destLoc: slot, srcLoc: const int
            else if constexpr (same_as<T, QLocResult_Slot> && same_as<U, QReadResult_ConstInt32>)
            {
                auto* intType = contexts.bodyContext.GetIntType();

                // dest = const
                contexts.bodyContext.EmitInst(QInst_Assign{intType, QArg_Dest{destLoc.slotIndex}, QArg_Value_ConstInt32{src.value}});

                if (o_destSlotIndex)
                {
                    // result = const
                    contexts.bodyContext.EmitInst(QInst_Assign{intType, QArg_Dest{*o_destSlotIndex}, QArg_Value_ConstInt32{src.value}});
                }

                return QEmitState_Ready{};
            }

            // 2-1. destLoc: ptr, srcLoc: slot
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_Slot>)
            {
                // *dest = src
                auto* type = contexts.bodyContext.GetSlotType(src.slotIndex);
                contexts.bodyContext.EmitInst(QInst_Store{type, QArg_Addr_PtrSlot{destLoc.slotIndex}, QArg_Value_Slot{src.slotIndex}});

                if (o_destSlotIndex)
                {
                    // result = src
                    contexts.bodyContext.EmitInst(QInst_Assign{type, QArg_Dest{*o_destSlotIndex}, QArg_Value_Slot{src.slotIndex}});
                }

                return QEmitState_Ready{};
            }
            // 2-2. destLoc: ptr, srcLoc: ptr
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_Ptr>)
            {   
                auto* type = contexts.bodyContext.GetSlotType(destLoc.slotIndex);

                if (!o_destSlotIndex)
                {
                    size_t size = contexts.qAbi->GetTypeSize(type);
                    contexts.bodyContext.EmitIntrinsic(
                        QInst_IntrinsicKind::Memcpy_Void_Ptr_Ptr_Int, 
                        /*o_dest*/nullopt,
                        {QArg_CallArg_Slot{destLoc.slotIndex}, QArg_CallArg_Slot{src.slotIndex}, QArg_CallArg_ConstInt32{(int)size}});
                }
                else
                {
                    // result = *src                            
                    contexts.bodyContext.EmitInst(QInst_Load{type, QArg_Dest{*o_destSlotIndex}, QArg_Addr_PtrSlot{src.slotIndex}});

                    // *dest = result
                    contexts.bodyContext.EmitInst(QInst_Store{type, QArg_Addr_PtrSlot{destLoc.slotIndex}, QArg_Value_Slot{*o_destSlotIndex}});
                }

                return QEmitState_Ready{};
            }

            // 2-3. destLoc: ptr, srcLoc: const bool
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_ConstBool>)
            {
                auto* boolType = contexts.bodyContext.GetBoolType();

                // dest = const
                contexts.bodyContext.EmitInst(QInst_Store{boolType, QArg_Addr_PtrSlot{destLoc.slotIndex}, QArg_Value_ConstBool{src.value}});

                if (o_destSlotIndex)
                {
                    // result = const
                    contexts.bodyContext.EmitInst(QInst_Assign{boolType, QArg_Dest{*o_destSlotIndex}, QArg_Value_ConstBool{src.value}});
                }

                return QEmitState_Ready{};
            }

            // 2-4. destLoc: ptr, srcLoc: const int
            else if constexpr (same_as<T, QLocResult_Ptr> && same_as<U, QReadResult_ConstInt32>)
            {
                auto* intType = contexts.bodyContext.GetIntType();

                // dest = const
                contexts.bodyContext.EmitInst(QInst_Store{intType, QArg_Addr_PtrSlot{destLoc.slotIndex}, QArg_Value_ConstInt32{src.value}});

                if (o_destSlotIndex)
                {
                    // result = const
                    contexts.bodyContext.EmitInst(QInst_Assign{intType, QArg_Dest{*o_destSlotIndex}, QArg_Value_ConstInt32{src.value}});
                }

                return QEmitState_Ready{};
            }
            else static_assert(false);

        }, **e_s_dest, **e_s_src);
    }

    ResultType Visit(MExp_Stmt* exp) 
    {
        auto e_s_result = TranslateMStmtsToQInsts(exp->stmts, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_result);

        // exp->finalExp는 context dependent하다. 여기는 MCreate_BC를 Translation하기 위한 곳이므로, finalExp를 MCreate로 간주하고 Translate한다
        return Accept(MCreate_BCQInstsTranslator{o_destSlotIndex, contexts}, exp->finalExp);
    }

    // &x
    ResultType Visit(MExp_PtrRef* exp)
    {
        auto e_s_loc = TranslateMLocToQInsts(exp->innerLoc, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_loc);

        if (!o_destSlotIndex) return QEmitState_Ready{};

        return visit([this](auto& loc) -> ResultType {
            using T = remove_cvref_t<decltype(loc)>;

            if constexpr (same_as<T, QLocResult_Slot>)
            {
                // slot의 addrof를 하나 한다 ptr 타입
                contexts.bodyContext.EmitInst(QInst_AddrOf{QArg_Dest{*o_destSlotIndex}, loc.slotIndex});
            }
            else if constexpr (same_as<T, QLocResult_Ptr>)
            {
                // ptr이면, destSlotIndex에 넣어준다
                auto* ptrType = contexts.bodyContext.GetPtrType();
                contexts.bodyContext.EmitInst(QInst_Assign{ptrType, QArg_Dest{*o_destSlotIndex}, QArg_Value_Slot{loc.slotIndex}});
            }
            else static_assert(false);

            return QEmitState_Ready{};
        }, **e_s_loc);
    }

    ResultType Visit(MExp_BoolLiteral* exp) 
    {
        if (!o_destSlotIndex) return QEmitState_Ready{}; // nested가 없으니 바로 리턴한다

        contexts.bodyContext.EmitInst(QInst_Assign{contexts.bodyContext.GetBoolType(), QArg_Dest{*o_destSlotIndex}, QArg_Value_ConstBool{exp->value}});
        return QEmitState_Ready{};
    }

    ResultType Visit(MExp_IntLiteral* exp) 
    { 
        if (!o_destSlotIndex) return QEmitState_Ready{}; // nested가 없으니 바로 리턴한다

        contexts.bodyContext.EmitInst(QInst_Assign{contexts.bodyContext.GetIntType(), QArg_Dest{*o_destSlotIndex}, QArg_Value_ConstInt32{exp->value}});
        return QEmitState_Ready{};
    }

    ResultType Visit(MExp_CallIntrinsic* exp) 
    {
        auto* intrinsicInfo = GetIntrinsicInfo(exp->kind, &*contexts.rFactory);
        auto e_s_o_retSlotIndex = HandleIntrinsicCall(intrinsicInfo, o_destSlotIndex, exp->typeArgs, exp->args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_o_retSlotIndex);
        return QEmitState_Ready{};
    }

    ResultType Visit(MExp_Call* exp) 
    { 
        auto e_s_result = HandleCall(exp->callable.decl, exp->callable.typeArgs, o_destSlotIndex, exp->callable.o_instance, exp->args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_result);

        return QEmitState_Ready{};
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
        return HandleInlineBlock(exp->scope, o_destSlotIndex, contexts);
    }
    // ResultType Visit(MExp_Is* exp) {}
};

struct MCreate_NBCQInstsTranslator
{
    using ResultType = expected<QEmitState<void>, DiagPtr>;
    optional<size_t> o_destSlotIndex;
    QTranslationContexts& contexts;

    ResultType Visit(MInitExp* initExp)
    {
        throw NotImplementedException{};
    }

    // ResultType Visit(MInitExp_Shared* mInitExp) { }
    // ResultType Visit(MInitExp_SharedRef* mInitExp) { }
    // ResultType Visit(MInitExp_Stmt* mInitExp) { }
    ResultType Visit(MInitExp_String* mInitExp) 
    { 
        return TranslateMInitExp_StringToQInsts(mInitExp, o_destSlotIndex, contexts);
    }
    // ResultType Visit(MInitExp_List* mInitExp) { }
    ResultType Visit(MInitExp_CallIntrinsic* mInitExp) 
    { 
        auto* intrinsicInfo = GetIntrinsicInfo(mInitExp->kind, &*contexts.rFactory);
        auto e_s_o_retSlotIndex = HandleIntrinsicCall(intrinsicInfo, o_destSlotIndex, mInitExp->typeArgs, mInitExp->args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_o_retSlotIndex);
        return QEmitState_Ready{};
    }
    // ResultType Visit(MInitExp_NewClass* mInitExp) { }
    ResultType Visit(MInitExp_StructCtor* mInitExp) 
    {
        // TODO: [61] 일반적인 struct ctor, dtor, copy/move ctor, copy/move assign 구현
        auto* type = GetType(mInitExp, &*contexts.rFactory);
        if (type != contexts.bodyContext.GetStringType())
            throw NotImplementedException{};

        size_t destSlotIndex = o_destSlotIndex ? *o_destSlotIndex : contexts.bodyContext.NewSlot(type);
        
        return visit([this, destSlotIndex](auto& kind) -> ResultType {
            using T = remove_cvref_t<decltype(kind)>;
            if constexpr (same_as<T, MInitExp_StructCtorKind_Copy>)
            {
                auto e_s_srcResult = TranslateMLocToQInsts(kind.src.loc, contexts);
                RETURN_ON_ERROR_OR_DONE(e_s_srcResult);

                auto srcArg = MakeAddrCallArg(**e_s_srcResult, contexts);

                contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::CopyCtor_Void_StringRef_StringInRef, nullopt, {
                    QArg_CallArg_AddrOfSlot{destSlotIndex},
                    srcArg
                });

                return QEmitState_Ready{};
            }
            else if constexpr (same_as<T, MInitExp_StructCtorKind_Move>)
            {
                // TODO: [30] move구현
                throw NotImplementedException{};
            }
            else if constexpr (same_as<T, MInitExp_StructCtorKind_General>)
            {
                // TODO: [61] 일반적인 struct ctor, dtor, copy/move ctor, copy/move assign 구현
                throw NotImplementedException{};
            }
            else static_assert(false);

        }, mInitExp->kind);
    }

    ResultType Visit(MInitExp_Call* mInitExp)
    { 
        auto e_s_result = HandleCall(mInitExp->callable.decl, mInitExp->callable.typeArgs, o_destSlotIndex, mInitExp->callable.o_instance, mInitExp->args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_result);

        return QEmitState_Ready{};
    }

    // ResultType Visit(MInitExp_NewEnumElem* mInitExp) { }
    // ResultType Visit(MInitExp_Nullable* mInitExp) { }
    // ResultType Visit(MInitExp_NullableNullLiteral* mInitExp) { }
    // ResultType Visit(MInitExp_NullableInplaceNullLiteral* mInitExp) { }
    // ResultType Visit(MInitExp_Cast* mInitExp) { }
    // ResultType Visit(MInitExp_Lambda* mInitExp) { }
    ResultType Visit(MInitExp_InlineBlock* mInitExp) 
    { 
        return HandleInlineBlock(mInitExp->scope, o_destSlotIndex, contexts);
    }
    // ResultType Visit(MInitExp_As* mInitExp) { }
};

expected<QEmitState<void>, DiagPtr> TranslateMCreate_NBCToQInsts(MInitExp* mInitExp, optional<size_t> o_destSlotIndex, QTranslationContexts& contexts)
{
    return Accept(MCreate_NBCQInstsTranslator{o_destSlotIndex, contexts}, mInitExp);
}

expected<QEmitState<void>, DiagPtr> TranslateMCreateToQInsts(MCreate& mCreate, optional<size_t> o_destSlotIndex, QTranslationContexts& contexts)
{
    return visit([&o_destSlotIndex, &contexts](auto& mCreate) -> expected<QEmitState<void>, DiagPtr> {
        using T = remove_cvref_t<decltype(mCreate)>;

        if constexpr (same_as<T, MCreate_BC>)
        {
            return Accept(MCreate_BCQInstsTranslator{o_destSlotIndex, contexts}, mCreate.exp);
        }
        else if constexpr (same_as<T, MCreate_NBC>)
        {
            return Accept(MCreate_NBCQInstsTranslator{o_destSlotIndex, contexts}, mCreate.initExp);
        }
        else static_assert(false);
        
    }, mCreate);
}

} // namespace Citron