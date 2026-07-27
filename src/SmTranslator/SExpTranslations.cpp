#include "SExpTranslations.h"
#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RFactory.h"
#include "MIR/MInitExp.h"
#include "MIR/MCreate.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"
#include "MIR/MStmt.h"
#include "MIR/MInitExp.h"
#include "MIR/MFactory.h"
#include "ReExp.h"
#include "SExpToReExp.h"
#include "ReExpToMIR.h"
#include "SExpToIrExp.h"
#include "IrExpToMSharedExp.h"
#include "SmTranslationContexts.h"
#include "SmScopeContext.h"
#include "DesignatedDiagnostic.h"
#include "BinOpQueryService.h"
#include "Misc.h"
#include "SStmtToMStmt.h"

using namespace std;

namespace Citron {

expected<MInitExp_StringElem, DiagPtr> TranslateSStringExpElementToMInitExp_StringElem(SStringExpElement* elem, SmTranslationContexts& contexts)
{
    if (auto* expElem = dynamic_cast<SStringExpElement_Exp*>(elem))
    {
        auto e_reExp = TranslateSExpToReExp(expElem->exp, /*hintType*/nullptr, contexts);
        RETURN_ON_ERROR(e_reExp);

        auto* reExpType = GetType(*e_reExp, &*contexts.rFactory);

        // 캐스팅이 필요하다면 
        if (reExpType == contexts.rFactory->MakeIntType())
        {
            auto e_mCreateInt = TranslateReExpToMCreate(*e_reExp, contexts);
            RETURN_ON_ERROR(e_mCreateInt);

            // int였으므로 무조건 BC
            auto& mCreateIntBC = get<MCreate_BC>(*e_mCreateInt);
            auto* typeArgs = contexts.rFactory->MakeEmptyTypeArguments();
            std::vector<MArgument> args;
            args.push_back(MArgument_Create{move(mCreateIntBC)});

            auto* initExp = contexts.mFactory->MakeMInitExp<MInitExp_CallIntrinsic>(
                MInitExp_CallIntrinsicKind::ToString_String_Int, typeArgs, move(args));
            
            return MInitExp_StringElem_InitExp{initExp};
        }
        else if (reExpType == contexts.rFactory->MakeBoolType())
        {
            auto e_mCreateBool = TranslateReExpToMCreate(*e_reExp, contexts);
            RETURN_ON_ERROR(e_mCreateBool);

            auto& mCreateBoolBC = get<MCreate_BC>(*e_mCreateBool);
            auto* typeArgs = contexts.rFactory->MakeEmptyTypeArguments();
            std::vector<MArgument> args;
            args.push_back(MArgument_Create{move(mCreateBoolBC)});

            auto* initExp = contexts.mFactory->MakeMInitExp<MInitExp_CallIntrinsic>(
                MInitExp_CallIntrinsicKind::ToString_String_Bool, typeArgs, move(args));

            return MInitExp_StringElem_InitExp{initExp};
        }
        else if (reExpType == contexts.rFactory->MakeStringType())
        {   
            return visit([](auto& reExp) -> expected<MInitExp_StringElem, DiagPtr> {
                using T = remove_cvref_t<decltype(reExp)>;

                if constexpr (same_as<T, ReExp_Loc>) return MInitExp_StringElem_Loc{reExp.mLoc};
                else if constexpr (same_as<T, ReExp_Exp>) throw RuntimeFatalException{};
                else if constexpr (same_as<T, ReExp_InitExp>) return MInitExp_StringElem_InitExp{reExp.mInitExp};
                else if constexpr (same_as<T, ReExp_StmtCall>) throw RuntimeFatalException{};
                else if constexpr (same_as<T, ReExp_StmtAssign>) throw RuntimeFatalException{};
                else static_assert(false);

            }, *e_reExp);
        }
        else
        {
            // TODO: [45] custom ToString 구현
            return Error<Error_StringExp_ExpElementShouldBeBoolOrIntOrString>();
        }
    }
    else if (auto* textElem = dynamic_cast<SStringExpElement_Text*>(elem))
    {
        return MInitExp_StringElem_Text(textElem->text);
    }

    unreachable();
}

expected<MInitExp_String*, DiagPtr> TranslateSExp_StringToMInitExp_String(SExp_String* sExp, SmTranslationContexts& contexts)
{
    vector<DiagPtr> diags;
    vector<MInitExp_StringElem> mElems;
    for (auto& elem : sExp->elements)
    {
        auto e_mElem = TranslateSStringExpElementToMInitExp_StringElem(elem, contexts);

        if (!e_mElem)
        {
            diags.push_back(e_mElem.error());
            continue;
        }

        mElems.push_back(move(*e_mElem));
    }

    if (!diags.empty())
        return Error<AggregateDiag>(move(diags));

    return contexts.mFactory->MakeMInitExp<MInitExp_String>(move(mElems));
}

expected<MExp_IntLiteral*, DiagPtr> TranslateSExp_IntLiteralToMExp_IntLiteral(SExp_IntLiteral* sExp, SmTranslationContexts& contexts)
{
    return contexts.mFactory->MakeMExp<MExp_IntLiteral>(sExp->value);
}

expected<MExp_BoolLiteral*, DiagPtr> TranslateSExp_BoolLiteralToMExp_BoolLiteral(SExp_BoolLiteral* sExp, SmTranslationContexts& contexts)
{
    return contexts.mFactory->MakeMExp<MExp_BoolLiteral>(sExp->value);
}

expected<ReExp, DiagPtr> TranslateSExp_NullLiteralToReExp(SExp_NullLiteral* sExp, RType* hintType, SmTranslationContexts& contexts)
{
    // hintType이 없으면 null을 만들수가 없다
    // var a = null; (x)

    if (!hintType) return Error<Error_NullLiteralExp_CantInferNullableType>();

    auto copyStrategy = hintType->GetCopyStrategy();

    switch (copyStrategy)
    {
    case RCopyStrategy::Void: throw RuntimeFatalException{}; // nullable<> 타입인데, void일수가 없다
    case RCopyStrategy::Bitwise:
    {
        // int? i = null;
        if (auto* nullableHintType = dynamic_cast<RType_Nullable*>(hintType))
            return ReExp_Exp{contexts.mFactory->MakeMExp<MExp_NullableNullLiteral>(nullableHintType->innerType)};

        return Error<Error_NullLiteralExp_CantInferNullableType>();
    }

    case RCopyStrategy::NonBitwise:
    {
        // S? s = null;
        if (auto* nullableHintType = dynamic_cast<RType_Nullable*>(hintType))
            return ReExp_InitExp{contexts.mFactory->MakeMInitExp<MInitExp_NullableNullLiteral>(nullableHintType->innerType)};

        // C? c = null;
        if (auto* nullableInplaceHintType = dynamic_cast<RType_NullableInplace*>(hintType))
            return ReExp_InitExp{contexts.mFactory->MakeMInitExp<MInitExp_NullableInplaceNullLiteral>(nullableInplaceHintType->innerType)};

        return Error<Error_NullLiteralExp_CantInferNullableType>();
    }
    }

    unreachable();
}

expected<ReExp, DiagPtr> TranslateSExp_BinaryOp_AssignToReExp(SExp_BinaryOp* sExp, SmTranslationContexts& contexts)
{
    // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다
    DesignatedDiagnostic<Error_BinaryOp_LeftOperandIsNotAssignable> designatedDiag;
    auto e_mDestLoc = TranslateSExpToMLoc(sExp->operand0, /*hintType*/ nullptr, /*bMaterializeExp*/false, &designatedDiag, contexts);
    RETURN_ON_ERROR(e_mDestLoc);

    // 안되는거 체크
    auto* mDestLoc = *e_mDestLoc;
    if (dynamic_cast<MLoc_LambdaVar*>(mDestLoc))
    {
        // TODO: [46] lambda capture에 reference들어올수 있도록
        // int x = 0; var l = [&x] () { x = 3; }, 가능하도록
        return Error<Error_BinaryOp_LeftOperandIsNotAssignable>();
    }
    else if (dynamic_cast<MLoc_This*>(mDestLoc))
    {
        return Error<Error_BinaryOp_LeftOperandIsNotAssignable>();
    }
    else if (dynamic_cast<MLoc_Materialize*>(mDestLoc))
    {
        return Error<Error_BinaryOp_LeftOperandIsNotAssignable>();
    }

    auto* mDestLocType = GetType(mDestLoc, &*contexts.rFactory);

    auto e_mSrc = TranslateSExpToMRead(sExp->operand1, /*hintType*/mDestLocType, contexts);
    RETURN_ON_ERROR(e_mSrc);

    return visit([&contexts, mDestLoc, mDestLocType](auto& mSrc) -> expected<ReExp, DiagPtr> {
        using T = remove_cvref_t<decltype(mSrc)>;

        if constexpr (same_as<T, MRead_Exp>)
        {
            auto* rType = GetType(mSrc.exp, &*contexts.rFactory);
            if (rType != mDestLocType)
            {
                // TODO: [47] CastMExp의 리턴값 수정, NBC의 암시적 Cast구현하기
                /*auto e_mCastExp = CastMExp(mSrc.exp, mDestLocType, contexts);
                RETURN_ON_ERROR(e_mCastExp);*/

                return ReExp_Exp{contexts.mFactory->MakeMExp<MExp_Store>(mDestLoc, MRead_Exp{mSrc.exp})};
            }
            return ReExp_Exp{contexts.mFactory->MakeMExp<MExp_Store>(mDestLoc, move(mSrc))};
        }
        else if constexpr (same_as<T, MRead_Loc>)
        {
            auto* rType = GetType(mSrc.loc, &*contexts.rFactory);
            if (rType != mDestLocType)
            {
                // TODO: [47] CastMExp의 리턴값 수정, NBC의 암시적 Cast구현하기
                throw NotImplementedException{};
            }

            // TODO: [40] MInitExp_StructCtorKind_*, MStmt_Assign를 쓸때 Copy, Move가 가능한지 확인하고 fallback까지 하는 코드 작성
            return ReExp_StmtAssign{contexts.mFactory->MakeMStmt<MStmt_Assign>(MTopLevel_Assign{mDestLoc, MStmt_AssignKind_Copy{move(mSrc)}})};
        }
        else static_assert(false);
    }, *e_mSrc);
}

optional<MArgument> TryMakeBinOpMArgument(ReExp& reExp, SmTranslationContexts& contexts)
{
    RType* type = GetType(reExp, &*contexts.rFactory);

    // 파라미터를 다음과 같이 생각함
    // 그리고 move는 안한다고 생각
    // int (BC) -> int => MArgument_Create(MCreate_BC(MExp_Load(l))) MArgument_Create(MCreate_BC(exp))
    // string (NBC) -> [in]string& => location일 경우 MArgument_Location(l) or InitExp일 경우 MArgument_Location(MLoc_Materialize(MCreate_NBC(initExp)))

    switch (type->GetCopyStrategy())
    {
    case RCopyStrategy::Void: throw RuntimeFatalException{};
    case RCopyStrategy::Bitwise:
    {
        return visit([&contexts](auto& reExp) -> optional<MArgument> {
            using T = remove_cvref_t<decltype(reExp)>;
            if constexpr (same_as<T, ReExp_Loc>)
            {
                auto* mExp = contexts.mFactory->MakeMExp<MExp_Load>(reExp.mLoc);
                return MArgument_Create{MCreate_BC{mExp}};
            }
            else if constexpr (same_as<T, ReExp_Exp>)
            {
                return MArgument_Create{MCreate_BC{reExp.mExp}};
            }
            else return nullopt;
        }, reExp);
    }
    case RCopyStrategy::NonBitwise:
    {
        return visit([&contexts](auto& reExp) -> optional<MArgument> {
            using T = remove_cvref_t<decltype(reExp)>;
            if constexpr (same_as<T, ReExp_Loc>)
            {
                return MArgument_Loc{reExp.mLoc};
            }
            else if constexpr (same_as<T, ReExp_InitExp>)
            {
                auto* mLoc = contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_NBC{reExp.mInitExp});
                return MArgument_Loc{mLoc};
            }
            else return nullopt;
        }, reExp);
    }
    }

    unreachable();
}

optional<ReExp> TryMatchBinOp(ReExp& operand0, ReExp& operand1, RType* operandType0, RType* operandType1, const BinOpInfo& info, SmTranslationContexts& contexts)
{
    // TODO: [48] CastMExp등, 시도만 하고 포인터를 버리는 경우, 메모리 누수를 막기 위해서, 지역 pool을 만들어서 flush처리, 성공시 pool merge
    // TODO: [47] CastMExp의 리턴값 수정, NBC의 암시적 Cast구현하기
    /*auto e_castExp0 = CastMExp(operand0, info.operandType0, contexts);
    if (!e_castExp0) continue;

    auto e_castExp1 = CastMExp(operand1, info.operandType1, contexts);
    if (!e_castExp1) continue;*/

    // 지금은 타입이 정확히 같을 경우에만 처리하도록
    if (operandType0 != info.operandType0) return nullopt;
    if (operandType1 != info.operandType1) return nullopt;

    // 인자를 모두 읽기전용으로 생각한다 T(BC) 혹은 [in]T& (NBC)
    // operandType이 BC면, MArgument_Create{Create_BC{}}
    // operandType이 NBC면, MArgument_Loc{l} 또는 MArgument_Loc{MLoc_Materialize{MCreate_NBC{}}
    auto o_arg0 = TryMakeBinOpMArgument(operand0, contexts);
    if (!o_arg0) return nullopt;

    auto o_arg1 = TryMakeBinOpMArgument(operand1, contexts);
    if (!o_arg1) return nullopt;

    // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
    return visit([&contexts, &o_arg0, &o_arg1](auto& _operator) -> ReExp {
        using T = remove_cvref_t<decltype(_operator)>;

        auto* typeArgs = contexts.rFactory->MakeEmptyTypeArguments();
        std::vector<MArgument> args;
        args.reserve(2);
        args.push_back(move(*o_arg0));
        args.push_back(move(*o_arg1));

        if constexpr (same_as<T, BinOp_Exp>)
            return ReExp_Exp{contexts.mFactory->MakeMExp<MExp_CallIntrinsic>(_operator.kind, typeArgs, move(args))};
        else if constexpr (same_as<T, BinOp_InitExp>)
            return ReExp_InitExp{contexts.mFactory->MakeMInitExp<MInitExp_CallIntrinsic>(_operator.kind, typeArgs, move(args))};
        else static_assert(false);
    }, info._operator);
}

expected<ReExp, DiagPtr> TranslateSExp_BinaryOpToReExp(SExp_BinaryOp* sExp, SmTranslationContexts& contexts)
{
    // 1. Assign 먼저 처리
    if (sExp->kind == SBinaryOpKind::Assign)
        return TranslateSExp_BinaryOp_AssignToReExp(sExp, contexts);

    auto e_reOperand0 = TranslateSExpToReExp(sExp->operand0, /*hintType*/nullptr, contexts);
    RETURN_ON_ERROR(e_reOperand0);
    auto* operandType0 = GetType(*e_reOperand0, &*contexts.rFactory);

    auto e_reOperand1 = TranslateSExpToReExp(sExp->operand1, /*hintType*/nullptr, contexts);
    RETURN_ON_ERROR(e_reOperand1);
    auto* operandType1 = GetType(*e_reOperand0, &*contexts.rFactory);

    // 2. NotEqual 처리
    if (sExp->kind == SBinaryOpKind::NotEqual)
    {
        const auto& equalInfos = contexts.binOpQueryService->GetInfos(SBinaryOpKind::Equal);

        for (auto& info : equalInfos)
        {
            auto o_reExp = TryMatchBinOp(*e_reOperand0, *e_reOperand1, operandType0, operandType1, info, contexts);
            if (!o_reExp) continue;

            auto o_arg = TryMakeBinOpMArgument(*o_reExp, contexts);
            if (!o_arg) continue;

            auto* typeArgs = contexts.rFactory->MakeEmptyTypeArguments();
            vector<MArgument> args;
            args.reserve(1);
            args.push_back(*o_arg);
            return ReExp_Exp{contexts.mFactory->MakeMExp<MExp_CallIntrinsic>(MExp_CallIntrinsicKind::LogicalNot_Bool_Bool, typeArgs, move(args))};
        }

        return Error<Error_BinaryOp_OperatorNotFound>();
    }

    // 3. InternalOperator에서 검색            
    auto matchedInfos = contexts.binOpQueryService->GetInfos(sExp->kind);
    for (auto& info : matchedInfos)
    {
        auto o_reExp = TryMatchBinOp(*e_reOperand0, *e_reOperand1, operandType0, operandType1, info, contexts);
        if (!o_reExp) continue;

        // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
        return *o_reExp;
    }

    // Operator를 찾을 수 없습니다
    return Error<Error_BinaryOp_OperatorNotFound>();
}


// int만 지원한다
expected<ReExp, DiagPtr> TranslateSExp_UnaryOp_AssignToReExp(ReExp& reOperand, MExp_CallIntrinsicKind op, SmTranslationContexts& contexts)
{
    // exp를 loc으로 변환하는 일을 하면 안되지만, ref는 풀어야 한다
    // F()++; (x)
    // var& x = i; x++; (o)

    DesignatedDiagnostic<Error_UnaryAssignOp_AssignableExpressionIsAllowedOnly> designatedDiag;
    auto e_mOperandLoc = TranslateReExpToMLoc(reOperand, /*bMaterializeExp*/false, &designatedDiag, contexts);
    RETURN_ON_ERROR(e_mOperandLoc);

    auto* type = GetType(*e_mOperandLoc, &*contexts.rFactory);

    // int type 검사, exact match
    if (type != contexts.rFactory->MakeIntType())
        return Error<Error_UnaryAssignOp_AssignableExpressionIsAllowedOnly>();

    // 
    auto* typeArgs = contexts.rFactory->MakeEmptyTypeArguments();
    vector<MArgument> args;
    args.reserve(1);
    args.push_back(MArgument_Loc{*e_mOperandLoc}); // location으로 넘겨주기
    auto* mExp = contexts.mFactory->MakeMExp<MExp_CallIntrinsic>(op, typeArgs, move(args));
    return ReExp_Exp{mExp};
}

expected<ReExp, DiagPtr> TranslateSExp_UnaryOpToReExp(SExp_UnaryOp* sExp, RType* hintType, SmTranslationContexts& contexts)
{
    // *x
    if (sExp->kind == SUnaryOpKind::Deref)
    {
        auto e_reOperand = TranslateSExpToReExp(sExp->operand, /*hintType*/nullptr, contexts);
        RETURN_ON_ERROR(e_reOperand);

        auto* type = GetType(*e_reOperand, &*contexts.rFactory);
        if (dynamic_cast<RType_Ptr*>(type))
        {
            auto e_srcPtr = TranslateReExpToMRead(*e_reOperand, contexts);
            RETURN_ON_ERROR(e_srcPtr);

            // RType_Ptr인데, BC가 안나오면 이상한
            return ReExp_Loc{contexts.mFactory->MakeMLoc<MLoc_PtrDeref>(move(*e_srcPtr))};
        }
        else if (dynamic_cast<RType_Shared*>(type))
        {
            auto e_srcShared = TranslateReExpToMRead(*e_reOperand, contexts);
            RETURN_ON_ERROR(e_srcShared);

            // RType_Shared이므로, MRead_Loc으로 얻어올수 있다
            auto& srcSharedLoc = get<MRead_Loc>(*e_srcShared);
            return ReExp_Loc{contexts.mFactory->MakeMLoc<MLoc_SharedDeref>(move(srcSharedLoc))};
        }
    }

    // &x 처리
    if (sExp->kind == SUnaryOpKind::Ref)
    {
        // hintType 따라 분기
        if (!hintType) return Error<Error_UnaryOp_RefNeedHintType>();

        // hintType 따라서 분기를 한다
        if (auto* ptrHintType = dynamic_cast<RType_Ptr*>(hintType))
        {
            // operand 분석시에 hintType을 넣는게 맞는지
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> notLocationDiag;
            auto e_loc = TranslateSExpToMLoc(sExp->operand, ptrHintType->innerType, /*bMaterializeExp*/false, &notLocationDiag, contexts);
            RETURN_ON_ERROR(e_loc);

            return ReExp_Exp{contexts.mFactory->MakeMExp<MExp_PtrRef>(*e_loc)};
        }
        else if (auto* sharedHintType = dynamic_cast<RType_Shared*>(hintType))
        {
            auto e_sharedExp = TranslateSExpToMSharedExp(sExp->operand, contexts);
            RETURN_ON_ERROR(e_sharedExp);

            return ReExp_InitExp{contexts.mFactory->MakeMInitExp<MInitExp_SharedRef>(*e_sharedExp)};
        }
        else
        {
            return Error<Error_UnaryOp_RefHintTypeShouldBePtrOrShared>();
        }
    }

    auto e_reOperand = TranslateSExpToReExp(sExp->operand, /*hintType*/nullptr, contexts);
    RETURN_ON_ERROR(e_reOperand);
    auto* type = GetType(*e_reOperand, &*contexts.rFactory);
    auto* typeArgs = contexts.rFactory->MakeEmptyTypeArguments();

    switch (sExp->kind)
    {
    case SUnaryOpKind::Deref:
    case SUnaryOpKind::Ref:
        unreachable();

    case SUnaryOpKind::LogicalNot:
    {
        // exact match
        if (type != contexts.rFactory->MakeBoolType())
            return Error<Error_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly>();

        auto e_mCreate = TranslateReExpToMCreate(*e_reOperand, contexts);
        RETURN_ON_ERROR(e_mCreate);

        vector<MArgument> args;
        args.reserve(1);
        args.push_back(MArgument_Create{move(*e_mCreate)});
        auto* mExp = contexts.mFactory->MakeMExp<MExp_CallIntrinsic>(MExp_CallIntrinsicKind::LogicalNot_Bool_Bool, typeArgs, move(args));
        return ReExp_Exp{mExp};
    }

    case SUnaryOpKind::Minus:
    {
        if (type != contexts.rFactory->MakeIntType())
            return Error<Error_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly>();

        auto e_mCreate = TranslateReExpToMCreate(*e_reOperand, contexts);
        RETURN_ON_ERROR(e_mCreate);

        vector<MArgument> args;
        args.reserve(1);
        args.push_back(MArgument_Create{move(*e_mCreate)});
        auto* mExp = contexts.mFactory->MakeMExp<MExp_CallIntrinsic>(MExp_CallIntrinsicKind::UnaryMinus_Int_Int, typeArgs, move(args));
        return ReExp_Exp{mExp};
    }

    case SUnaryOpKind::PostfixInc: // e.m++ 등
        return TranslateSExp_UnaryOp_AssignToReExp(*e_reOperand, MExp_CallIntrinsicKind::PostfixInc_Int_IntRef, contexts);

    case SUnaryOpKind::PostfixDec:
        return TranslateSExp_UnaryOp_AssignToReExp(*e_reOperand, MExp_CallIntrinsicKind::PostfixDec_Int_IntRef, contexts);

    case SUnaryOpKind::PrefixInc:
        return TranslateSExp_UnaryOp_AssignToReExp(*e_reOperand, MExp_CallIntrinsicKind::PrefixInc_Int_IntRef, contexts);

    case SUnaryOpKind::PrefixDec:
        return TranslateSExp_UnaryOp_AssignToReExp(*e_reOperand, MExp_CallIntrinsicKind::PrefixDec_Int_IntRef, contexts);
    }

    unreachable();
}

expected<ReExp, DiagPtr> TranslateSExp_LambdaToReExp(SExp_Lambda* sExp, SmTranslationContexts& contexts)
{
    // TODO: 리턴 타입과 인자타입은 타입 힌트를 반영해야 한다
    //RType* retType = nullptr;

    //auto o_lambdaInfo = TranslateLambda(retType, sExp->params, sExp->body, contexts);

    //if (!o_lambdaInfo)
    //    return nullptr;

    // return MakePtr<NLambdaExp>(lambdaInfo.lambda, lambdaInfo.args), contexts.factory->MakeIn);
    throw NotImplementedException{};
}

expected<MLoc_ListIndexer*, DiagPtr> TranslateSExp_IndexerToMLoc_ListIndexer(SExp_Indexer* sExp, SmTranslationContexts& contexts)
{
    auto e_mObj = TranslateSExpToMRead(sExp->obj, /*hintType*/nullptr, contexts);
    RETURN_ON_ERROR(e_mObj);

    auto e_mIndex = TranslateSExpToMRead(sExp->index, /*hintType*/nullptr, contexts);
    RETURN_ON_ERROR(e_mIndex);

    auto* objType = GetType(*e_mObj, &*contexts.rFactory);
    auto* indexType = GetType(*e_mIndex, &*contexts.rFactory);
    auto* intType = contexts.rFactory->MakeIntType();

    // TODO: [49] Dictionary 추가, indexer도 dictionary지원

    // 타입 체크
    RType* itemType;
    if (!contexts.rFactory->IsListType(objType, &itemType))
        return Error<Error_Indexer_ObjectShouldBeListOrDictionary>();

    if (indexType != intType)
        return Error<Error_Indexer_IndexTypeNotMatched>();

    // ListType이라면 Loc으로 변환할 수 있다
    MRead_Loc& mObjLoc = get<MRead_Loc>(*e_mObj);

    // 리스트 타입이라면 NBC, int타입이라면 BC가 확정이다
    return contexts.mFactory->MakeMLoc<MLoc_ListIndexer>(move(mObjLoc), move(*e_mIndex), itemType);
}

expected<MInitExp*, DiagPtr> TranslateSExp_ListToMInitExp(SExp_List* sExp, RType* hintType, SmTranslationContexts& contexts)
{
    vector<MCreate> elems;
    elems.reserve(sExp->elements.size());

    // TODO: 타입 힌트도 이용해야 할 것 같다
    RType* curElemType;
    if (!contexts.rFactory->IsListType(hintType, &curElemType))
        curElemType = nullptr;

    for (auto& sElem : sExp->elements)
    {
        auto e_mCreate = TranslateSExpToMCreate(sElem, /*hintType*/curElemType, contexts);
        RETURN_ON_ERROR_REFDECL(e_mCreate, mCreate);

        auto* rElemType = GetType(mCreate, &*contexts.rFactory);

        if (curElemType == nullptr)
            curElemType = rElemType;
        else if (curElemType != rElemType)
            return Error<Error_ListExp_MismatchBetweenElementTypes>();

        elems.push_back(move(mCreate));
    }

    if (curElemType == nullptr)
        return Error<Error_ListExp_CantInferElementTypeWithEmptyElement>();

    return contexts.mFactory->MakeMInitExp<MInitExp_List>(move(elems), curElemType);
}

// 클래스 만들기
expected<MInitExp_NewClass*, DiagPtr> TranslateSExp_NewToMInitExp_NewClass(SExp_New* sExp, SmTranslationContexts& contexts)
{
    auto e_rType = contexts.scopeContext->TranslateSTypeExpToRType(sExp->type);
    RETURN_ON_ERROR(e_rType);

    if ((*e_rType)->GetTypeKind() == RTypeKind::Class)
        return Error<Error_NewExp_TypeIsNotClass>();

    throw NotImplementedException{};
    //var classDecl = classSymbol.GetDecl();

    //var candidates = FuncCandidateSMake&<ClassConstructorDeclSymbol, ClassConstructorSymbol>(
    //    classSymbol, classDecl.GetConstructorCount(), classDecl.GetConstructor, partialTypeArgs: default); // TODO: 일단은 constructor의 typeArgs는 없는 것으로

    //var matchResult = FuncsMatcher.Match(candidates, exp->Args, contexts);
    //if (matchResult == null)
    //    throw NotImplementedException{}; // 매치에 실패했습니다.

    //var(constructor, args) = matchResult.Value;
    //return Valid(new IR0ExpResult(new R.NewClassExp(constructor, args), new ClassType(classSymbol)));
}

expected<MInitExp_Shared*, DiagPtr> TranslateSExp_SharedToMInitExp_Shared(SExp_Shared* sExp, RType* hintType, SmTranslationContexts& contexts)
{
    auto* hintSharedType = dynamic_cast<RType_Shared*>(hintType);
    auto* innerHintType = hintSharedType ? hintSharedType->innerType : nullptr;

    // hintType 전수
    auto e_mCreate = TranslateSExpToMCreate(sExp->innerExp, innerHintType, contexts);
    RETURN_ON_ERROR(e_mCreate);

    return contexts.mFactory->MakeMInitExp<MInitExp_Shared>(move(*e_mCreate));
}

// a is B
expected<MExp_Is*, DiagPtr> TranslateSExp_IsToMExp_Is(SExp_Is* sExp, SmTranslationContexts& contexts)
{
    // TODO: [50] is에 pattern, alias binding추가
    throw NotImplementedException{};
    //auto e_target = TranslateSExpToMExp(sExp->exp, /*hintType*/nullptr, contexts);
    //RETURN_ON_ERROR(e_target);

    //auto* targetType = (*e_target)->GetType();
    //auto targetTypeKind = targetType->GetTypeKind();

    //auto e_testType = contexts.scopeContext->TranslateSTypeExpToRType(sExp->type);
    //RETURN_ON_ERROR(e_testType);

    //auto testTypeKind = (*e_testType)->GetTypeKind();

    //// 5가지 케이스로 나뉜다
    //if (testTypeKind == RTypeKind::Class)
    //{
    //    if (targetTypeKind == RTypeKind::Class)
    //        return contexts.mFactory->MakeMExp<MExp_ClassIsClass>(*e_target, *e_testType, contexts.rFactory);
    //    else if (targetTypeKind == RTypeKind::Interface)
    //        return contexts.mFactory->MakeMExp<MExp_InterfaceIsClass>(*e_target, *e_testType, contexts.rFactory);
    //    else
    //        throw NotImplementedException{}; // 에러 처리
    //}
    //else if (testTypeKind == RTypeKind::Interface)
    //{
    //    if (targetTypeKind == RTypeKind::Class)
    //        return contexts.mFactory->MakeMExp<MExp_ClassIsInterface>(*e_target, *e_testType, contexts.rFactory);
    //    else if (targetTypeKind == RTypeKind::Interface)
    //        return contexts.mFactory->MakeMExp<MExp_InterfaceIsInterface>(*e_target, *e_testType, contexts.rFactory);
    //    else
    //        throw NotImplementedException{}; // 에러 처리
    //}
    //else if (auto* enumElemTestType = dynamic_cast<RType_EnumElem*>(*e_testType))
    //{
    //    if (dynamic_cast<RType_Enum*>(targetType))
    //        return contexts.mFactory->MakeMExp<MExp_EnumIsEnumElem>(*e_target, enumElemTestType, contexts.rFactory);
    //    else
    //        throw NotImplementedException{}; // 에러 처리

    //}
    //else
    //    throw NotImplementedException{}; // 에러 처리
}

expected<MInitExp_As*, DiagPtr> TranslateSExp_AsToMInitExp_As(SExp_As* sExp, SmTranslationContexts& contexts)
{
    // MInitExp_AsKind::
    auto e_mTarget = TranslateSExpToMRead(sExp->exp, /* hintType */ nullptr, contexts);
    RETURN_ON_ERROR(e_mTarget);

    auto e_rTestType = contexts.scopeContext->TranslateSTypeExpToRType(sExp->type);
    RETURN_ON_ERROR(e_rTestType);

    return MakeMInitExp_As(move(*e_mTarget), *e_rTestType, contexts);
}

expected<ReExp, DiagPtr> TranslateSExp_InlineToReExp(SExp_Inline* sExp, RType* hintType, SmTranslationContexts& contexts)
{
    return UsingTranslationContexts_InlineScope(nullopt, hintType, contexts, [sExp, hintType](MScopeKind_Inline scopeKind, SmTranslationContexts& contexts) -> expected<ReExp, DiagPtr> {

        // leave return type
        vector<MStmt*> mStmts;
        auto e_result = TranslateSStmtsToMStmts(mStmts, sExp->stmts, contexts);
        RETURN_ON_ERROR(e_result);

        if (sExp->o_finalExp)
        {
            auto* o_rHintType = contexts.scopeContext->GetInlineScopeType();
            auto e_create = TranslateSExpToMCreate(sExp->o_finalExp, o_rHintType, contexts);
            RETURN_ON_ERROR(e_create);

            auto* leaveType = GetType(*e_create, &*contexts.rFactory);

            // Translate 하다가 바뀌었을 수도 있으니
            auto* o_rType = contexts.scopeContext->GetInlineScopeType();
            if (!o_rType)
            {   
                contexts.scopeContext->SetInlineScopeType(leaveType);
                o_rType = leaveType;
            }
            else
            {
                // 타입 체크
                if (o_rType != leaveType)
                    return Error<Error_InlineExp_TypeMismatch>();
            }

            auto* mStmt = contexts.mFactory->MakeMStmt<MStmt_Leave>(scopeKind.labelId, MTopLevel_Create{move(*e_create)});
            mStmts.push_back(mStmt);
        }

        auto* o_rType = contexts.scopeContext->GetInlineScopeType();
        if (!o_rType) return Error<Error_InlineExp_CantDecideType>();

        switch (o_rType->GetCopyStrategy())
        {
        case RCopyStrategy::Void: return Error<Error_InlineExp_DoesntAllowVoid>();
        case RCopyStrategy::Bitwise:
        {
            auto* mScope = contexts.mFactory->MakeMStmt<MStmt_Scope>(move(scopeKind), move(mStmts));
            auto* mExp = contexts.mFactory->MakeMExp<MExp_InlineBlock>(mScope, o_rType);
            return ReExp_Exp{mExp};
        }
        case RCopyStrategy::NonBitwise:
        {
            auto* mScope = contexts.mFactory->MakeMStmt<MStmt_Scope>(move(scopeKind), move(mStmts));
            auto* mInitExp = contexts.mFactory->MakeMInitExp<MInitExp_InlineBlock>(mScope, o_rType);
            return ReExp_InitExp{mInitExp};
        }
        }
        unreachable();
    });
}

expected<MCreate, DiagPtr> TranslateSExpToMCreate(SExp* sExp, RType* hintType, SmTranslationContexts& contexts)
{
    auto e_reExp = TranslateSExpToReExp(sExp, hintType, contexts);
    RETURN_ON_ERROR(e_reExp);

    return TranslateReExpToMCreate(*e_reExp, contexts);
}

expected<MRead, DiagPtr> TranslateSExpToMRead(SExp* sExp, RType* hintType, SmTranslationContexts& contexts)
{
    auto e_reExp = TranslateSExpToReExp(sExp, hintType, contexts);
    RETURN_ON_ERROR(e_reExp);

    return TranslateReExpToMRead(*e_reExp, contexts);
}

expected<MLoc*, DiagPtr> TranslateSExpToMLoc(SExp* sExp, RType* hintType, bool bMaterializeExp, IDesignatedDiagnostic* notLocationDiag, SmTranslationContexts& contexts)
{
    auto e_reExp = TranslateSExpToReExp(sExp, hintType, contexts);
    RETURN_ON_ERROR(e_reExp);

    return TranslateReExpToMLoc(*e_reExp, bMaterializeExp, notLocationDiag, contexts);
}

expected<MSharedExp*, DiagPtr> TranslateSExpToMSharedExp(SExp* sExp, SmTranslationContexts& contexts)
{
    auto e_irExp = TranslateSExpToIrExp(sExp, contexts);
    RETURN_ON_ERROR(e_irExp);

    return TranslateIrExpToMSharedExp(*e_irExp, contexts);
}




} // namespace Citron