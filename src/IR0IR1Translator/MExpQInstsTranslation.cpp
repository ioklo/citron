#include "MExpQInstsTranslation.h"

#include <expected>
#include <variant>
#include <vector>
#include <unordered_map>

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"
#include "Infra/Unreachable.h"

#include "Logging/Diag.h"

#include "RSymbol/RGlobalFuncDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RTypes.h"

#include "MIR/MExp.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QInsts.h"
#include "QIR/QArgs.h"

#include "MLocQInstsTranslation.h"
#include "MStmtQInstsTranslation.h"
#include "QBodyContext.h"
#include "ScopeGuard.h"
#include "CommonQInstsTranslation.h"

using namespace std;

namespace Citron {

// srcSlotIndex는 src를 지칭하는 slot
// struct S { int x; } int a;
// S s; s.x = a; -> EmitInitLValue(&s.x, a, RType_Struct{S}, bodyContext);
expected<void, DiagPtr> EmitInitLValue(size_t destPtrSlotIndex, size_t srcSlotIndex, RType* type, QBodyContext& bodyContext)
{
    // 1. primitive인지 확인
    if (auto* primitiveType = dynamic_cast<RType_Primitive*>(type))
    {
        // primitive면 바로 store
        return bodyContext.EmitInst(QInst_Store{type, QArg_Slot{destPtrSlotIndex}, QArg_Slot{srcSlotIndex}});
    }

    // 2. struct라면 copy ctor
    else if (auto* structType = dynamic_cast<RType_Struct*>(type))
    {
        // struct이면 copy ctor를 호출
        auto* ctor = structType->decl->GetUnboundCopyCtor();
        assert(ctor);

        size_t srcPtrSlotIndex = bodyContext.NewSlot(bodyContext.GetPtrType(structType));
        auto e_result = bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{srcPtrSlotIndex}, QArg_Slot{srcSlotIndex}});
        assert(e_result);

        vector<QArg_Input> args;
        args.push_back(QArg_Slot{destPtrSlotIndex}); // this ptr
        args.push_back(QArg_Slot{srcPtrSlotIndex});  // src ptr
        return bodyContext.EmitInst(QInst_Call{ctor, nullopt, move(args)});
    }
    else throw NotImplementedException{};
}

expected<void, DiagPtr> EmitInitLValuePtr(size_t destPtrSlotIndex, size_t srcPtrSlotIndex, RType* type, QBodyContext& bodyContext)
{
    // 1. primitive인지 확인
    if (auto* primitiveType = dynamic_cast<RType_Primitive*>(type))
    {
        // primitive면 바로 store
        size_t srcSlotIndex = bodyContext.NewSlot(type);
        bodyContext.EmitInst(QInst_Load{type, QArg_Slot{srcSlotIndex}, QArg_Slot{srcPtrSlotIndex}});
        return bodyContext.EmitInst(QInst_Store{type, QArg_Slot{destPtrSlotIndex}, QArg_Slot{srcSlotIndex}});
    }

    // 2. struct라면 copy ctor
    else if (auto* structType = dynamic_cast<RType_Struct*>(type))
    {
        // struct이면 copy ctor를 호출
        auto* ctor = structType->decl->GetUnboundCopyCtor();
        assert(ctor);

        vector<QArg_Input> args;
        args.push_back(QArg_Slot{destPtrSlotIndex}); // this ptr
        args.push_back(QArg_Slot{srcPtrSlotIndex});  // src ptr
        return bodyContext.EmitInst(QInst_Call{ctor, nullopt, move(args)});
    }
    else throw NotImplementedException{};
}

// MExp는 QInst와 1:1로 대응되지는 않는다
class MExpQInstsTranslator
{
public:
    using ResultType = expected<void, DiagPtr>;

private:
    optional<size_t> o_destSlotIndex;
    QBodyContext& bodyContext;

public:
    MExpQInstsTranslator(optional<size_t> o_destSlotIndex, QBodyContext& bodyContext)
        : o_destSlotIndex{o_destSlotIndex}, bodyContext{bodyContext}
    {
    }

    // load(loc),
    ResultType Visit(MExp_Load* exp)
    {
        // 이 translation으로 lv가 하나 나올 것이다
        auto e_srcLoc = TranslateMLocToQInsts(exp->loc, bodyContext); // ptr이 담긴 slot
        RETURN_ON_ERROR(e_srcLoc);

        // dest를 할당할일이 없으면, loc까지만 실행하고 종료
        if (!o_destSlotIndex) return {};

        return visit([this, exp](auto& srcLoc) -> expected<void, DiagPtr>
        {
            using T = remove_cvref_t<decltype(srcLoc)>;
            if constexpr (same_as<T, QLocResult_Slot>)
            {
                auto* type = GetType(exp, bodyContext.rFactory);

                // TODO: String을 일반적인 struct로
                if (type == bodyContext.GetStringType())
                {
                    // string이면 string의 Copy Assign을 불러줘야 한다.
                    return bodyContext.EmitInst(QInst_CopyAssign_String{*o_destSlotIndex, QArg_Slot{srcLoc.slotIndex}});
                }

                // primitive는 assign을 해도 된다 => CopyAssign으로 대체해야할지도
                return bodyContext.EmitInst(QInst_Assign{type, QArg_Slot{*o_destSlotIndex}, QArg_Slot{srcLoc.slotIndex}});
            }
            else if constexpr (same_as<T, QLocResult_PtrSlot>)
            {
                auto* type = GetType(exp, &*contexts.rFactory);
                // ptr에서 로드하는 식으로
                return bodyContext.EmitInst(QInst_Load{type, QArg_Slot{*o_destSlotIndex}, QArg_Slot{srcLoc.slotIndex}});
            }
            else static_assert(false);

        }, *e_srcLoc);

        return {};
    }

    // Assign(loc dest, src exp), Store
    // *loc = exp;
    ResultType Visit(MExp_Assign* exp)
    {
        // dest를 먼저 계산한다
        auto e_dest = TranslateMLocToQInsts(exp->dest, bodyContext);
        RETURN_ON_ERROR(e_dest);
        
        return visit([this, exp](auto& destLoc) -> expected<void, DiagPtr>
        {
            using T = remove_cvref_t<decltype(destLoc)>;
            if constexpr (same_as <T, QLocResult_Slot>)
            {
                // slot이면 바로 exp계산에 참여시킬수 있다
                auto e_src = TranslateMExpToQInsts(exp->src, destLoc.slotIndex, bodyContext);
                RETURN_ON_ERROR(e_src);

                // *oDest에 *eDest를 넣어야 한다
                // *oDestSlotIndex: T, *e_dest: T
                if (o_destSlotIndex)
                {
                    auto* type = exp->src->GetType();
                    auto e_emitResult = bodyContext.EmitInst(QInst_Assign{type, *o_destSlotIndex, QArg_Slot{destLoc.slotIndex}});
                    RETURN_ON_ERROR(e_emitResult);
                }

                return {};
            }
            else if constexpr (same_as<T, QLocResult_PtrSlot>)
            {
                auto* type = exp->src->GetType();
                size_t slotIndex = o_destSlotIndex ? *o_destSlotIndex : bodyContext.NewSlot(type);

                // slot이면 바로 exp계산에 참여시킬수 있다
                auto e_src = TranslateMExpToQInsts(exp->src, slotIndex, bodyContext);
                RETURN_ON_ERROR(e_src);

                auto e_store = bodyContext.EmitInst(QInst_Store{type, QArg_Slot{destLoc.slotIndex}, QArg_Slot{slotIndex}});
                RETURN_ON_ERROR(e_store);

                return {};
            }
            else static_assert(false); // 나머지는 추후에

        }, *e_dest);
    }

    ResultType Visit(MExp_Stmt* exp)
    {
        auto e_result = TranslateMStmtsToQInsts(exp->stmts, bodyContext);
        RETURN_ON_ERROR(e_result);

        return TranslateMExpToQInsts(exp->finalExp, o_destSlotIndex, bodyContext);
    }

    // shared 3;
    // IR1에서 힙으로 값을 올리려면,
    // runtime 함수를 콜 하고,
    // 그 위치에 exp를 넣도록
    ResultType Visit(MExp_Shared* exp)
    {
        throw NotImplementedException{};

        //// 1. alloc, size
        //auto* type = exp->innerExp->GetType();
        // auto size = bodyContext.GetTypeSize(type);
        //auto ptr = bodyContext.AddIntrinsic(QInst_IntrinsicKind::Alloc_Int, {QArg_ConstInt32{(int)size}});

        //// 2. exp
        //auto e_qValue = TranslateMExpToQInsts(exp->innerExp, bodyContext);
        //RETURN_ON_ERROR(e_qValue);

        //// 3. 저장
        //bodyContext.AddInst(QInst_Store{.dest = ptr, .src = *eQValue});
        //return *eQValue;
    }

    // PtrRef (Loc)
    ResultType Visit(MExp_PtrRef* exp)
    {
        //// ptr ref는 &s.a 같은 걸 수 있다
        //auto e_lv = TranslateMLocToQInsts(exp->innerLoc, bodyContext);
        //RETURN_ON_ERROR(e_lv);

        //// lv 그대로 리턴하면 될거 같다
        //return *e_lv;
        throw NotImplementedException{};
    }

    ResultType Visit(MExp_BoolLiteral* exp)
    {
        if (!o_destSlotIndex) return {}; // nested가 없으니 바로 리턴한다

        return bodyContext.EmitInst(QInst_Assign{bodyContext.GetBoolType(), *o_destSlotIndex, QArg_ConstBool{exp->value}});
    }

    ResultType Visit(MExp_IntLiteral* exp)
    {
        if (!o_destSlotIndex) return {};

        return bodyContext.EmitInst(QInst_Assign{bodyContext.GetIntType(), *o_destSlotIndex, QArg_ConstInt32{exp->value}});
    }

    ResultType Visit(MExp_String* exp)
    {
        if (o_destSlotIndex)
            return TranslateMExp_StringToQInsts(exp, *o_destSlotIndex, bodyContext);

        return TranslateMExp_StringToQInsts(exp, nullopt, bodyContext);
    }

    ResultType Visit(MExp_List* exp)
    {
        /*std::vector<QArg_Value> items;
        items.reserve(exp->elems.size());

        for (auto* elem : exp->elems)
        {
            auto e_item = TranslateMExpToQInsts(elem, bodyContext);
            RETURN_ON_ERROR(e_item);

            items.push_back(*e_item);
        }

        auto result = bodyContext.AddIntrinsic(QInst_IntrinsicKind::NewList_Items, std::move(items));
        return *result;*/
        throw NotImplementedException{};
    }

    ResultType Visit(MExp_ListIterator* exp)
    {
        /*auto e_lv = TranslateMLocToQInsts(exp->listLoc, bodyContext);
        RETURN_ON_ERROR(e_lv);

        auto result = bodyContext.AddIntrinsic(QInst_IntrinsicKind::GetListIterator_List, {*e_lv});
        return *result;*/
        throw NotImplementedException{};
    }

    ResultType Visit(MExp_CallInternalUnaryOperator* exp)
    {   
        auto operandSlotIndex = bodyContext.NewSlot(exp->operand->GetType());
        auto e_operand = TranslateMExpToQInsts(exp->operand, operandSlotIndex, bodyContext);
        RETURN_ON_ERROR(e_operand);

        static unordered_map<MInternalUnaryOperator, QInst_IntrinsicKind> m{
            {MInternalUnaryOperator::LogicalNot_Bool_Bool, QInst_IntrinsicKind::LogicalNot_Bool},
            {MInternalUnaryOperator::UnaryMinus_Int_Int, QInst_IntrinsicKind::UnaryMinus_Int},
            {MInternalUnaryOperator::ToString_Bool_String, QInst_IntrinsicKind::ToString_Bool},
            {MInternalUnaryOperator::ToString_Int_String, QInst_IntrinsicKind::ToString_Int}
        };

        auto i = m.find(exp->op);
        assert(i != m.end());

        if (o_destSlotIndex)
            return bodyContext.EmitIntrinsic(i->second, *o_destSlotIndex, {operandSlotIndex});

        return {};
    }

    ResultType Visit(MExp_CallInternalUnaryAssignOperator* exp)
    {
        // exp->operand는 항상 lvalue이다
        auto e_operand = TranslateMLocToQInsts(exp->operand, bodyContext);
        RETURN_ON_ERROR(e_operand);

        static unordered_map<MInternalUnaryAssignOperator, QInst_IntrinsicKind> m{
            {MInternalUnaryAssignOperator::PrefixInc_Int_Int, QInst_IntrinsicKind::PrefixInc_Int},
            {MInternalUnaryAssignOperator::PrefixDec_Int_Int, QInst_IntrinsicKind::PrefixDec_Int},
            {MInternalUnaryAssignOperator::PostfixInc_Int_Int, QInst_IntrinsicKind::PostfixInc_Int},
            {MInternalUnaryAssignOperator::PostfixDec_Int_Int, QInst_IntrinsicKind::PostfixDec_Int},
        };

        auto i = m.find(exp->op);
        assert(i != m.end());

        return visit([this, op = i->second](auto& operand) -> expected<void, DiagPtr> {

            using T = remove_cvref_t<decltype(operand)>;
            if constexpr (same_as<T, QLocResult_Slot>)
            {
                // slot to ptr
                auto* ptrType = bodyContext.GetPtrType();
                auto ptrSlotIndex = bodyContext.NewSlot(ptrType);
                auto e_emitAddrResult = bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{ptrSlotIndex}, QArg_Slot{operand.slotIndex}});
                RETURN_ON_ERROR(e_emitAddrResult);

                auto destSlotIndex = o_destSlotIndex ? *o_destSlotIndex : bodyContext.NewSlot(bodyContext.GetIntType());
                auto e_emitIntrinsicResult = bodyContext.EmitIntrinsic(op, QArg_Slot{destSlotIndex}, {QArg_Slot{ptrSlotIndex}});
                RETURN_ON_ERROR(e_emitIntrinsicResult);
                return {};
            }
            else if constexpr (same_as<T, QLocResult_PtrSlot>)
            {
                auto destSlotIndex = o_destSlotIndex ? *o_destSlotIndex : bodyContext.NewSlot(bodyContext.GetIntType());
                auto e_emitIntrinsicResult = bodyContext.EmitIntrinsic(op, QArg_Slot{destSlotIndex}, {QArg_Slot{operand.slotIndex}});
                RETURN_ON_ERROR(e_emitIntrinsicResult);
                return {};
            }
            else static_assert(false);

        }, *e_operand);
    }

    ResultType Visit(MExp_CallInternalBinaryOperator* exp)
    {
        auto operandSlotIndex0 = bodyContext.NewSlot(exp->operand0->GetType());
        auto e_operand0 = TranslateMExpToQInsts(exp->operand0, operandSlotIndex0, bodyContext);
        RETURN_ON_ERROR(e_operand0);

        auto operandSlotIndex1 = bodyContext.NewSlot(exp->operand1->GetType());
        auto e_operand1 = TranslateMExpToQInsts(exp->operand1, operandSlotIndex1, bodyContext);
        RETURN_ON_ERROR(e_operand1);

        static unordered_map<MInternalBinaryOperator, QInst_IntrinsicKind> m{
            {MInternalBinaryOperator::Multiply_Int_Int_Int, QInst_IntrinsicKind::Multiply_Int_Int},
            {MInternalBinaryOperator::Divide_Int_Int_Int, QInst_IntrinsicKind::Divide_Int_Int},
            {MInternalBinaryOperator::Modulo_Int_Int_Int, QInst_IntrinsicKind::Modulo_Int_Int},
            {MInternalBinaryOperator::Add_Int_Int_Int, QInst_IntrinsicKind::Add_Int_Int},
            {MInternalBinaryOperator::Add_String_String_String, QInst_IntrinsicKind::Add_String_String},
            {MInternalBinaryOperator::Subtract_Int_Int_Int, QInst_IntrinsicKind::Subtract_Int_Int},
            {MInternalBinaryOperator::LessThan_Int_Int_Bool, QInst_IntrinsicKind::LessThan_Int_Int},
            {MInternalBinaryOperator::LessThan_String_String_Bool, QInst_IntrinsicKind::LessThan_String_String},
            {MInternalBinaryOperator::GreaterThan_Int_Int_Bool, QInst_IntrinsicKind::GreaterThan_Int_Int},
            {MInternalBinaryOperator::GreaterThan_String_String_Bool, QInst_IntrinsicKind::GreaterThan_String_String},
            {MInternalBinaryOperator::LessThanOrEqual_Int_Int_Bool, QInst_IntrinsicKind::LessThanOrEqual_Int_Int},
            {MInternalBinaryOperator::LessThanOrEqual_String_String_Bool, QInst_IntrinsicKind::LessThanOrEqual_String_String},
            {MInternalBinaryOperator::GreaterThanOrEqual_Int_Int_Bool, QInst_IntrinsicKind::GreaterThanOrEqual_Int_Int},
            {MInternalBinaryOperator::GreaterThanOrEqual_String_String_Bool, QInst_IntrinsicKind::GreaterThanOrEqual_String_String},
            {MInternalBinaryOperator::Equal_Int_Int_Bool, QInst_IntrinsicKind::Equal_Int_Int},
            {MInternalBinaryOperator::Equal_Bool_Bool_Bool, QInst_IntrinsicKind::Equal_Bool_Bool},
            {MInternalBinaryOperator::Equal_String_String_Bool, QInst_IntrinsicKind::Equal_String_String},
        };

        auto i = m.find(exp->op);
        assert(i != m.end());

        if (o_destSlotIndex)
            return bodyContext.EmitIntrinsic(i->second, *o_destSlotIndex, {operandSlotIndex0, operandSlotIndex1});

        return {};
    }

    // GlobalFunc
    ResultType Visit(MExp_CallGlobalFunc* exp)
    {
        // generics는 어떻게 하나요
        // T F<T>(T t) { return t; }
        // Generics는 T에 관한 정보를 더 넘겨준다 (크기 등)
        // 따라서 이 함수는 t, {F함수에 대한 constraint table} 두 인자를 받는다
        // 그리고 t는 항상 stack pointer를 가리키게 된다 (callee쪽에서 크기를 정확히 알 수 없으므로)

        vector<QArg_Input> args;

        // 1. 결과값 선 처리, 결과값이 stack을 쓰는 경우라면 args에 첫 인자로 넣어준다
        struct RetPolicy_Void {};
        struct RetPolicy_UsingStackSlot { RType* slotType; };
        using RetPolicy = variant<RetPolicy_Void, RetPolicy_UsingStackSlot>;

        auto retPolicy = [this, exp, &args] -> RetPolicy {
            auto* retType = exp->rFuncDecl->GetReturnType(*exp->rTypeArgs);

            if (bodyContext.IsVoidType(retType))
            {
                assert(!o_destSlotIndex);
                return RetPolicy_Void{};
            }

            return RetPolicy_UsingStackSlot{retType};
        }();

        // 2. 인자를 args에 넣는다
        for (auto& arg : exp->args)
        {
            auto e_result = visit([this, &args](auto& arg) -> expected<void, DiagPtr>
            {
                using T = remove_cvref_t<decltype(arg)>;

                if constexpr (same_as<T, MArgument_Exp>)
                {
                    auto argDestSlotIndex = bodyContext.NewSlot(arg.exp->GetType());
                    auto e_result = TranslateMExpToQInsts(arg.exp, argDestSlotIndex, bodyContext);
                    RETURN_ON_ERROR(e_result);
                    args.push_back(QArg_Slot{argDestSlotIndex});
                    return {};
                }
                else if constexpr (same_as <T, MArgument_Ref>) // ref는 pointer로 넘긴다
                {
                    auto e_locResult = TranslateMLocToQInsts(arg.loc, bodyContext);
                    RETURN_ON_ERROR(e_locResult);

                    return visit([this, &args](auto& locResult) -> expected<void, DiagPtr>
                    {
                        using U = remove_cvref_t<decltype(locResult)>;

                        if constexpr (same_as<U, QLocResult_Slot>)
                        {
                            // slot이면 addrOf를 써서 ptr로 만든다
                            auto* rPtrType = bodyContext.GetPtrType();
                            auto ptrSlotIndex = bodyContext.NewSlot(rPtrType);
                            auto e_emitAddrResult = bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{ptrSlotIndex}, QArg_Slot{locResult.slotIndex}});
                            RETURN_ON_ERROR(e_emitAddrResult);
                            args.push_back(QArg_Slot{ptrSlotIndex});
                            return {};
                        }
                        else if constexpr (same_as<U, QLocResult_PtrSlot>)
                        {
                            // ptr이면 그대로 넣어준다
                            args.push_back(QArg_Slot{locResult.slotIndex});
                            return {};
                        }
                        else static_assert(false);
                    }, * e_locResult);
                }
                else if constexpr (same_as<T, MArgument_Move>)
                {
                    // TODO: [30] move구현
                    throw NotImplementedException{};
                }
                else if constexpr (same_as<T, MArgument_Params>) // 파라미터를 여러개 받는 경우
                {
                    // TODO: [31] params 구현
                    throw NotImplementedException{};
                    return {};
                }
                else static_assert(false);
            }, arg);
            RETURN_ON_ERROR(e_result);
        }

        // 3. Emit처리
        return visit([this, exp, &args](auto& retPolicy) -> expected<void, DiagPtr>
        {
            using T = remove_cvref_t<decltype(retPolicy)>;
            if constexpr (same_as<T, RetPolicy_Void>)
            {
                return bodyContext.EmitInst(QInst_Call{exp->rFuncDecl, nullopt, move(args)});
            }
            else if constexpr (same_as<T, RetPolicy_UsingStackSlot>)
            {
                size_t resultSlotIndex = o_destSlotIndex ? *o_destSlotIndex : bodyContext.NewSlot(retPolicy.slotType);
                return bodyContext.EmitInst(QInst_Call{exp->rFuncDecl, QArg_Slot{resultSlotIndex}, move(args)});
            }
            else static_assert(false);

        }, retPolicy);
    }

    ResultType Visit(MExp_NewClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CallClassFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CastClass* exp) { throw NotImplementedException{}; }

    ResultType Visit(MExp_NewStruct* exp) 
    { 
        assert(o_destSlotIndex); // dest가 반드시 있어야 한다

        // 멤버와이즈 생성자 특수처리. 생성자 호출없이 바로 필드에 넣는다
        if (exp->ctor->GetKind() == RStructCtorKind::Memberwise)
        {   
            auto* destType = bodyContext.GetSlotType(*o_destSlotIndex);
            auto* destPtrType = bodyContext.GetPtrType(destType);
            size_t destPtrSlotIndex = bodyContext.NewSlot(destPtrType); // dummy ptr slot
            bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{destPtrSlotIndex}, QArg_Slot{*o_destSlotIndex}});

            auto* _struct = exp->ctor->GetStructDecl();
            auto rStructVars = _struct->GetRVars();
            auto rStructVarCount = rStructVars.size();
            assert(exp->args.size() == rStructVarCount);
            for (size_t i = 0; i < rStructVarCount; i++)
            {
                auto* rStructVar = rStructVars[i];
                auto* rStructVarType = rStructVar->GetDeclType(exp->typeArgs);
                auto fieldPtrSlotIndex = bodyContext.NewSlot(bodyContext.GetPtrType(rStructVarType));
                bodyContext.EmitInst(QInst_FieldOf{QArg_Slot{fieldPtrSlotIndex}, QArg_Slot{destPtrSlotIndex}, i});

                visit([this, rStructVarType, fieldPtrSlotIndex](auto& arg) -> expected<void, DiagPtr>
                {
                    using T = remove_cvref_t<decltype(arg)>;
                    if constexpr (same_as<T, MArgument_Exp>)
                    {
                        auto argSlotIndex = bodyContext.NewSlot(rStructVarType);
                        auto e_result = TranslateMExpToQInsts(arg.exp, argSlotIndex, bodyContext);
                        RETURN_ON_ERROR(e_result);

                        bodyContext.EmitInst(QInst_Store{rStructVarType, QArg_Slot{fieldPtrSlotIndex}, QArg_Slot{argSlotIndex}});
                        return {};
                    }
                    else if constexpr (same_as<T, MArgument_Ref>)
                    {
                        auto e_locResult = TranslateMLocToQInsts(arg.loc, bodyContext);
                        RETURN_ON_ERROR(e_locResult);
                        
                        return visit([this, rStructVarType, fieldPtrSlotIndex](auto& locResult) -> expected<void, DiagPtr>
                        {
                            using U = remove_cvref_t<decltype(locResult)>;
                            if constexpr (same_as<U, QLocResult_Slot>)
                                return EmitInitLValue(fieldPtrSlotIndex, locResult.slotIndex, rStructVarType, bodyContext);
                            else if constexpr (same_as<U, QLocResult_PtrSlot>)
                                return EmitInitLValuePtr(fieldPtrSlotIndex, locResult.slotIndex, rStructVarType, bodyContext);
                            else static_assert(false);
                        }, locResult);
                    }
                    else if constexpr (same_as<T, MArgument_Move>)
                    {
                        // TODO: [30] move구현
                    }
                    else static_assert(false);
                }, exp->args[i]);
            }

            return {};
        }
        else
        {
            // 일반 생성자
            vector<QArg_Input> args;
            for (auto* argExp : exp->ctorArgs)
            {
                auto argDestSlotIndex = bodyContext.NewSlot(argExp->GetType());
                auto e_result = TranslateMExpToQInsts(argExp, argDestSlotIndex, bodyContext);
                RETURN_ON_ERROR(e_result);
                args.push_back(QArg_Slot{argDestSlotIndex});
            }
            return bodyContext.EmitInst(QInst_CallStructCtor{exp->ctor, QArg_Slot{*o_destSlotIndex}, move(args)});
        }
        
    }

    ResultType Visit(MExp_CallStructFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NewEnumElem* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CastEnumElemToEnum* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_Nullable* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NullableNullLiteral* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NullableRefNullLiteral* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_Lambda* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CallLambda* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CastSharedLambdaToFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InlineBlock* exp) { throw NotImplementedException{}; }

    ResultType Visit(MExp_ClassIsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassAsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassIsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassAsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InterfaceIsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InterfaceAsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InterfaceIsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InterfaceAsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_EnumIsEnumElem* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_EnumAsEnumElem* exp) { throw NotImplementedException{}; }
};

expected<void, DiagPtr> TranslateMExpToQInsts(MExp* mExp, optional<size_t> o_destSlotIndex, QBodyContext& bodyContext)
{
    MExpQInstsTranslator translator{o_destSlotIndex, bodyContext};
    return Accept(translator, mExp);
}

expected<void, DiagPtr> TranslateMExpToQInstsWithNewScope(MExp* mExp, optional<size_t> o_destSlotIndex, QBodyContext& bodyContext)
{
    ScopeGuard expGuard{bodyContext};
    return TranslateMExpToQInsts(mExp, o_destSlotIndex, bodyContext);
}

} // namespace Citron
