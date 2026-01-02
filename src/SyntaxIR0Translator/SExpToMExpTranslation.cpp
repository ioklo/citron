#include "SExpToMExpTranslation.h"

#include <variant>
#include <cassert>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Unreachable.h"
#include "Infra/Expected.h"
#include "Logging/Logger.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"
#include "MIR/MFactory.h"

#include "ReExp.h"
#include "ImExp.h"

#include "SExpToMLocTranslation.h"
#include "SExpToReExpTranslation.h"
#include "SExpToImExpTranslation.h"
#include "SExpRefToMExpTranslation.h"

#include "ReExpToMExpTranslation.h"
#include "ReExpToMLocTranslation.h"

#include "ImCallableAndSArgsToMExpTranslation.h"

#include "ScopeContext.h"
#include "Misc.h"
#include "BinOpQueryService.h"
#include "TranslationContexts.h"
#include "DesignatedDiagnostic.h"

using namespace std;

namespace Citron {

// Syntax Exp -> IR0 Exp로 바꿔주는 기본적인 코드
// Deref를 적용하지 않는다. 따로 해주어야 한다

expected<MExp*, DiagPtr> TranslateSNullLiteralExpToMExp(SExp_NullLiteral* exp, RType* hintType, TranslationContexts& contexts)
{
    if (hintType != nullptr)
    {
        // int? i = null;
        if (dynamic_cast<RType_NullableValue*>(hintType))
            return contexts.mFactory->MakeMExp<MExp_NullableValueNullLiteral>(hintType, contexts.rFactory);

        // C? c = null;
        if (dynamic_cast<RType_NullableRef*>(hintType))
            return contexts.mFactory->MakeMExp<MExp_NullableRefNullLiteral>(hintType, contexts.rFactory);
    }

    // TODO: if (a == nullptr)도 반영해야 한다
    throw NotImplementedException{};
    return unexpected{MakePtr<Error_Reference_CantMakeReference>()};
}

expected<MExp*, DiagPtr> TranslateSBoolLiteralExpToMExp(SExp_BoolLiteral* exp, TranslationContexts& contexts)
{
    return contexts.mFactory->MakeMExp<MExp_BoolLiteral>(exp->value, contexts.rFactory);
}

expected<MExp*, DiagPtr> TranslateSIntLiteralExpToMExp(SExp_IntLiteral* exp, TranslationContexts& contexts)
{
    return contexts.mFactory->MakeMExp<MExp_IntLiteral>(exp->value, contexts.rFactory);
}

expected<MExp_StringElem, DiagPtr> TranslateSStringExpElementToRStringExpElement(SStringExpElement* elem, TranslationContexts& contexts)
{
    // TranslationResult<R.StringExpElement> Valid(R.StringExpElement elem) = > TranslationResult.Valid(elem);
    // TranslationResult<R.StringExpElement> Error() = > TranslationResult.Error<R.StringExpElement>();
    // var stringType = contexts.GetStringType();

    if (auto* expElem = dynamic_cast<SStringExpElement_Exp*>(elem))
    {
        auto e_reExp = TranslateSExpToReExp(expElem->exp, /* hintType */ nullptr, contexts);
        RETURN_ON_ERROR(e_reExp);

        auto reExpType = (*e_reExp)->GetType();

        // 캐스팅이 필요하다면 
        if (reExpType == contexts.rFactory->MakeIntType())
        {   
            auto e_nExp = TranslateReExpToMExp(*e_reExp, contexts);
            RETURN_ON_ERROR(e_nExp);

            return MExp_StringElem_Exp{contexts.mFactory->MakeMExp<MExp_CallInternalUnaryOperator>(MInternalUnaryOperator::ToString_Int_String, *e_nExp, contexts.rFactory)};
        }
        else if (reExpType == contexts.rFactory->MakeBoolType())
        {
            auto e_nExp = TranslateReExpToMExp(*e_reExp, contexts);
            RETURN_ON_ERROR(e_nExp);

            return MExp_StringElem_Exp{contexts.mFactory->MakeMExp<MExp_CallInternalUnaryOperator>(MInternalUnaryOperator::ToString_Bool_String, *e_nExp, contexts.rFactory)};
        }
        else if (reExpType == contexts.rFactory->MakeStringType())
        {
            auto e_mExp = TranslateReExpToMExp(*e_reExp, contexts);
            RETURN_ON_ERROR(e_mExp);

            return MExp_StringElem_Exp{*e_mExp};
        }
        else
        {
            // TODO: ToString
            return unexpected{MakePtr<Error_StringExp_ExpElementShouldBeBoolOrIntOrString>()};
        }
    }
    else if (auto* textElem = dynamic_cast<SStringExpElement_Text*>(elem))
    {
        return MExp_StringElem_Text(textElem->text);
    }

    unreachable();
}

expected<MExp_String*, DiagPtr> TranslateSStringExpToNStringExp(SExp_String* exp, TranslationContexts& contexts)
{
    vector<DiagPtr> diags;
    vector<MExp_StringElem> builder;
    for(auto& elem : exp->elements)
    {
        auto e_rStringExpElem = TranslateSStringExpElementToRStringExpElement(elem, contexts);

        if (!e_rStringExpElem)
        {
            diags.push_back(e_rStringExpElem.error());
            continue;
        }
        
        builder.push_back(move(*e_rStringExpElem));
    }

    if (!diags.empty())
        return unexpected{MakePtr<AggregateDiag>(move(diags))};

    return contexts.mFactory->MakeMExp<MExp_String>(move(builder), contexts.rFactory);
}

// int만 지원한다
expected<MExp*, DiagPtr> TranslateSIntUnaryAssignExpToMExp(SExp* operand, MInternalUnaryAssignOperator op, TranslationContexts& contexts)
{
    // exp를 loc으로 변환하는 일을 하면 안되지만, ref는 풀어야 한다
    // F()++; (x)
    // var& x = i; x++; (o)
    // throws NotLocationException
    
    DesignatedDiagnostic<Error_UnaryAssignOp_AssignableExpressionIsAllowedOnly> designatedDiag;
    auto e_nOperand = TranslateSExpToMLoc(operand, /* hintType */ nullptr, /* bWrapExpAsLoc */ false, &designatedDiag, contexts);
    RETURN_ON_ERROR(e_nOperand);

    // int type 검사, exact match
    if ((*e_nOperand)->GetType() != contexts.rFactory->MakeIntType())
        return unexpected{MakePtr<Error_UnaryAssignOp_AssignableExpressionIsAllowedOnly>()};

    return contexts.mFactory->MakeMExp<MExp_CallInternalUnaryAssignOperator>(op, *e_nOperand, contexts.rFactory);
}

expected<MExp*, DiagPtr> TranslateSUnaryOpExpToMExpExceptDeref(SExp_UnaryOp* sExp, TranslationContexts& contexts)
{
    assert(sExp->kind != SUnaryOpKind::Deref);

    // ref 처리
    if (sExp->kind == SUnaryOpKind::Ref)
        return TranslateSExpRefToMExp(sExp->operand, contexts);

    auto e_nOperand = TranslateSExpToMExp(sExp->operand, /*hintType*/ nullptr, contexts);
    RETURN_ON_ERROR(e_nOperand);

    switch(sExp->kind)
    {
    case SUnaryOpKind::LogicalNot:
    {
        // exact match
        if ((*e_nOperand)->GetType() != contexts.rFactory->MakeBoolType())
        {   
            return unexpected{MakePtr<Error_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly>()};
        }

        return contexts.mFactory->MakeMExp<MExp_CallInternalUnaryOperator>(MInternalUnaryOperator::LogicalNot_Bool_Bool, *e_nOperand, contexts.rFactory);
    }

    case SUnaryOpKind::Minus:
    {
        if ((*e_nOperand)->GetType() != contexts.rFactory->MakeIntType())
        {
            return unexpected{MakePtr<Error_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly>()};
        }

        return contexts.mFactory->MakeMExp<MExp_CallInternalUnaryOperator>(MInternalUnaryOperator::UnaryMinus_Int_Int, *e_nOperand, contexts.rFactory);
    }

    case SUnaryOpKind::PostfixInc: // e.m++ 등
        return TranslateSIntUnaryAssignExpToMExp(sExp->operand, MInternalUnaryAssignOperator::PostfixInc_Int_Int, contexts);

    case SUnaryOpKind::PostfixDec:
        return TranslateSIntUnaryAssignExpToMExp(sExp->operand, MInternalUnaryAssignOperator::PostfixDec_Int_Int, contexts);

    case SUnaryOpKind::PrefixInc:
        return TranslateSIntUnaryAssignExpToMExp(sExp->operand, MInternalUnaryAssignOperator::PrefixInc_Int_Int, contexts);

    case SUnaryOpKind::PrefixDec:
        return TranslateSIntUnaryAssignExpToMExp(sExp->operand, MInternalUnaryAssignOperator::PrefixDec_Int_Int, contexts);

    default:
        unreachable();
    }
}

expected<MExp*, DiagPtr> TranslateSAssignBinaryOpExpToMExp(SExp_BinaryOp* exp, TranslationContexts& contexts)
{
    // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다
    DesignatedDiagnostic<Error_BinaryOp_LeftOperandIsNotAssignable> designatedDiag;
    auto e_nDestLoc = TranslateSExpToMLoc(exp->operand0, /* hintType */ nullptr, /* bWrapExpAsLoc */ false, &designatedDiag, contexts);
    RETURN_ON_ERROR(e_nDestLoc);

    // 안되는거 체크
    auto* pNDestLoc = *e_nDestLoc;
    if (dynamic_cast<MLoc_LambdaVar*>(pNDestLoc))
    {
        // int x = 0; var l = () { x = 3; }, TODO: 이거 가능하도록
        return unexpected{MakePtr<Error_BinaryOp_LeftOperandIsNotAssignable>()};
    }
    else if (dynamic_cast<MLoc_This*>(pNDestLoc))
    {
        return unexpected{MakePtr<Error_BinaryOp_LeftOperandIsNotAssignable>()};
    }
    else if (dynamic_cast<MLoc_Temp*>(pNDestLoc))
    {
        return unexpected{MakePtr<Error_BinaryOp_LeftOperandIsNotAssignable>()};
    }

    auto nDestLocType = (*e_nDestLoc)->GetType();
    auto e_nSrcExp = TranslateSExpToMExp(exp->operand1, /*hintType*/ nDestLocType, contexts);
    RETURN_ON_ERROR(e_nSrcExp);

    auto e_nWrappedSrcExp = CastMExp(*e_nSrcExp, nDestLocType, contexts);
    RETURN_ON_ERROR(e_nWrappedSrcExp);

    return contexts.mFactory->MakeMExp<MExp_Assign>(*e_nDestLoc, *e_nWrappedSrcExp);
}

expected<MExp*, DiagPtr> TranslateSBinaryOpExpToMExp(SExp_BinaryOp* exp, TranslationContexts& contexts)
{
    // 1. Assign 먼저 처리
    if (exp->kind == SBinaryOpKind::Assign)
    {
        return TranslateSAssignBinaryOpExpToMExp(exp, contexts);
    }

    auto e_operand0 = TranslateSExpToMExp(exp->operand0, /*hintType*/ nullptr, contexts);
    RETURN_ON_ERROR(e_operand0);

    auto e_operand1 = TranslateSExpToMExp(exp->operand1, /*hintType*/ nullptr, contexts);
    RETURN_ON_ERROR(e_operand1);

    // 2. NotEqual 처리
    if (exp->kind == SBinaryOpKind::NotEqual)
    {
        const auto& equalInfos = contexts.binOpQueryService->GetInfos(SBinaryOpKind::Equal);
        
        for(auto& info : equalInfos)
        {
            auto castExp0 = CastMExp(*e_operand0, info.operandType0, contexts);
            if (!castExp0) continue;

            auto castExp1 = CastMExp(*e_operand1, info.operandType1, contexts);
            if (!castExp1) continue;

            // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
            auto* equalExp = contexts.mFactory->MakeMExp<MExp_CallInternalBinaryOperator>(info.rOperator, *castExp0, *castExp1, contexts.rFactory);
            return contexts.mFactory->MakeMExp<MExp_CallInternalUnaryOperator>(MInternalUnaryOperator::LogicalNot_Bool_Bool, equalExp, contexts.rFactory);
        }
    }

    // 3. InternalOperator에서 검색            
    auto matchedInfos = contexts.binOpQueryService->GetInfos(exp->kind);
    for(auto& info : matchedInfos)
    {
        auto castExp0 = CastMExp(*e_operand0, info.operandType0, contexts);
        if (!castExp0) continue;

        auto castExp1 = CastMExp(*e_operand1, info.operandType1, contexts);
        if (!castExp1) continue;

        // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다

        return contexts.mFactory->MakeMExp<MExp_CallInternalBinaryOperator>(info.rOperator, *castExp0, *castExp1, contexts.rFactory);
    }

    // Operator를 찾을 수 없습니다
    return unexpected{MakePtr<Error_BinaryOp_OperatorNotFound>()};
}

expected<MExp*, DiagPtr> TranslateSLambdaExpToMExp(SExp_Lambda* sExp, TranslationContexts& contexts)
{
    // TODO: 리턴 타입과 인자타입은 타입 힌트를 반영해야 한다
    //RType* retType = nullptr;
    
    //auto o_lambdaInfo = TranslateLambda(retType, sExp->params, sExp->body, contexts);

    //if (!o_lambdaInfo)
    //    return nullptr;

    // return MakePtr<NLambdaExp>(lambdaInfo.lambda, lambdaInfo.args), contexts.factory->MakeIn);
    throw NotImplementedException{};
}

expected<MExp*, DiagPtr> TranslateSListExpToMExp(SExp_List* exp, TranslationContexts& contexts)
{
    vector<MExp*> elems;
    elems.reserve(exp->elements.size());

    // TODO: 타입 힌트도 이용해야 할 것 같다
    RType* curElemType = nullptr;

    for(auto& elem : exp->elements)
    {
        auto e_nElem = TranslateSExpToMExp(elem, /*hintType*/ nullptr, contexts);
        RETURN_ON_ERROR(e_nElem);

        auto* rElemType = (*e_nElem)->GetType();
        elems.push_back(*e_nElem);

        if (curElemType == nullptr)
        {
            curElemType = rElemType;
            continue;
        }

        if (curElemType != rElemType)
        {
            return unexpected{MakePtr<Error_ListExp_MismatchBetweenElementTypes>()};
        }
    }

    if (curElemType == nullptr)
    {
        return unexpected{MakePtr<Error_ListExp_CantInferElementTypeWithEmptyElement>()};
    }

    return contexts.mFactory->MakeMExp<MExp_List>(move(elems), curElemType, contexts.rFactory);
}

expected<MExp*, DiagPtr> TranslateSNewExpToMExp(SExp_New* exp, TranslationContexts& contexts) // throws ErrorCodeException
{
    auto e_rType = contexts.scopeContext->TranslateSTypeExpToRType(exp->type);
    RETURN_ON_ERROR(e_rType);

    if ((*e_rType)->GetCustomTypeKind() == RCustomTypeKind::Class)
    {
        return unexpected{MakePtr<Error_NewExp_TypeIsNotClass>()};
    }

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

expected<MExp*, DiagPtr> TranslateSCallExpToMExp(SExp_Call* exp, RType* hintType, TranslationContexts& contexts)
{
    auto e_imCallable = TranslateSExpToImExp(exp->callable, hintType, contexts);
    RETURN_ON_ERROR(e_imCallable);

    return TranslateImCallableAndSArgsToMExp(*e_imCallable, exp->callable, exp->args, contexts); // 로깅할때 exp, exp->Callable두개가 다 필요할 수 있다
}

expected<MExp*, DiagPtr> TranslateSBoxExpToMExp(SExp_Box* exp, RType* hintType, TranslationContexts& contexts)
{
    auto* hintBoxType = dynamic_cast<RType_Box*>(hintType);
    auto* innerHintType = hintBoxType ? hintBoxType->innerType : nullptr;

    // hintType전수
    auto e_mInnerExp = TranslateSExpToMExp(exp->innerExp, innerHintType, contexts);
    RETURN_ON_ERROR(e_mInnerExp);

    return contexts.mFactory->MakeMExp<MExp_Box>(*e_mInnerExp, contexts.rFactory);
}

expected<MExp*, DiagPtr> TranslateSIsExpToMExp(SExp_Is* exp, TranslationContexts& contexts)
{
    auto e_target = TranslateSExpToMExp(exp->exp, /*hintType*/ nullptr, contexts);
    RETURN_ON_ERROR(e_target);

    auto targetType = (*e_target)->GetType();
    auto targetTypeKind = targetType->GetCustomTypeKind();

    auto e_testType = contexts.scopeContext->TranslateSTypeExpToRType(exp->type);
    RETURN_ON_ERROR(e_testType);

    auto testTypeKind = (*e_testType)->GetCustomTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RCustomTypeKind::Class)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return contexts.mFactory->MakeMExp<MExp_ClassIsClass>(*e_target, *e_testType, contexts.rFactory);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return contexts.mFactory->MakeMExp<MExp_InterfaceIsClass>(*e_target, *e_testType, contexts.rFactory);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::Interface)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return contexts.mFactory->MakeMExp<MExp_ClassIsInterface>(*e_target, *e_testType, contexts.rFactory);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return contexts.mFactory->MakeMExp<MExp_InterfaceIsInterface>(*e_target, *e_testType, contexts.rFactory);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::EnumElem)
    {
        if (targetTypeKind == RCustomTypeKind::Enum)
            return contexts.mFactory->MakeMExp<MExp_EnumIsEnumElem>(*e_target, *e_testType, contexts.rFactory);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else
        throw NotImplementedException{}; // 에러 처리
}

expected<MExp*, DiagPtr> TranslateSAsExpToMExp(SExp_As* exp, TranslationContexts& contexts)
{
    auto e_nTarget = TranslateSExpToMExp(exp->exp, /* hintType */ nullptr, contexts);
    RETURN_ON_ERROR(e_nTarget);

    auto e_nTestType = contexts.scopeContext->TranslateSTypeExpToRType(exp->type);
    RETURN_ON_ERROR(e_nTestType);

    return MakeMExp_As(*e_nTarget, *e_nTestType, contexts);
}

namespace {

// SExp -> MExp
class SExpToMExpTranslator
{
public:
    using ResultType = expected<MExp*, DiagPtr>;

private:
    RType* hintType;
    TranslationContexts& contexts;

public:
    SExpToMExpTranslator(RType* hintType, TranslationContexts& contexts)
        : hintType{hintType}, contexts{contexts}
    {
    }

private:
    // S.Exp -> IntermediateExp -> ResolvedExp -> R.Exp
    ResultType HandleDefault(SExp* exp)
    {
        auto e_reExp = TranslateSExpToReExp(exp, hintType, contexts);
        RETURN_ON_ERROR(e_reExp);

        return TranslateReExpToMExp(*e_reExp, contexts);
    }
    
public:
    ResultType Visit(SExp_Identifier* exp)
    {
        return HandleDefault(exp);
    }

    ResultType Visit(SExp_String* exp)
    {
        return TranslateSStringExpToNStringExp(exp, contexts);
    }

    ResultType Visit(SExp_IntLiteral* exp)
    {
        return TranslateSIntLiteralExpToMExp(exp, contexts);
    }

    ResultType Visit(SExp_BoolLiteral* exp)
    {
        return TranslateSBoolLiteralExpToMExp(exp, contexts);
    }

    ResultType Visit(SExp_NullLiteral* exp)
    {
        return TranslateSNullLiteralExpToMExp(exp, hintType, contexts);
    }

    ResultType Visit(SExp_BinaryOp* exp)
    {
        return TranslateSBinaryOpExpToMExp(exp, contexts);
    }

    ResultType Visit(SExp_UnaryOp* exp)
    {
        if (exp->kind == SUnaryOpKind::Deref)
            return HandleDefault(exp);

        return TranslateSUnaryOpExpToMExpExceptDeref(exp, contexts);
    }

    ResultType Visit(SExp_Call* exp)
    {
        return TranslateSCallExpToMExp(exp, hintType, contexts);
    }

    ResultType Visit(SExp_Lambda* exp)
    {
        return TranslateSLambdaExpToMExp(exp, contexts);
    }

    ResultType Visit(SExp_Indexer* exp)
    {
        return HandleDefault(exp);
    }

    ResultType Visit(SExp_Member* exp)
    {
        return HandleDefault(exp);
    }

    ResultType Visit(SExp_IndirectMember* exp)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(SExp_List* exp)
    {
        return TranslateSListExpToMExp(exp, contexts);
    }

    ResultType Visit(SExp_New* exp)
    {
        return TranslateSNewExpToMExp(exp, contexts);
    }

    ResultType Visit(SExp_Box* exp)
    {
        return TranslateSBoxExpToMExp(exp, hintType, contexts);
    }

    ResultType Visit(SExp_Is* exp)
    {
        return TranslateSIsExpToMExp(exp, contexts);
    }

    ResultType Visit(SExp_As* exp)
    {
        return TranslateSAsExpToMExp(exp, contexts);
    }
};

} // namespace 

expected<MExp*, DiagPtr> TranslateSExpToMExp(SExp* exp, RType* hintType, TranslationContexts& contexts)
{
    SExpToMExpTranslator translator{hintType, contexts};
    return Accept(translator, exp);
}

}