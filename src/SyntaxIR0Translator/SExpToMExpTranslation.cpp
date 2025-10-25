#include "SExpToMExpTranslation.h"

#include <variant>
#include <cassert>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Unreachable.h"
#include "Logging/Logger.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RTypes.h"
#include "MIR/MLoc.h"

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
#include "TranslationContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

// Syntax Exp -> IR0 Exp로 바꿔주는 기본적인 코드
// Deref를 적용하지 않는다. 따로 해주어야 한다

expected<MExp*, DiagPtr> TranslateSNullLiteralExpToMExp(SExp_NullLiteral* exp, RType* hintType, TranslationContext& context)
{
    if (hintType != nullptr)
    {
        // int? i = null;
        if (dynamic_cast<RType_NullableValue*>(hintType))
            return context.MakeMExp<MExp_NullableValueNullLiteral>(hintType);

        // C? c = null;
        if (dynamic_cast<RType_NullableRef*>(hintType))
            return context.MakeMExp<MExp_NullableRefNullLiteral>(hintType);
    }

    // TODO: if (a == nullptr)도 반영해야 한다
    throw NotImplementedException{};
    return unexpected{MakePtr<Error_Reference_CantMakeReference>()};
}

expected<MExp*, DiagPtr> TranslateSBoolLiteralExpToMExp(SExp_BoolLiteral* exp, TranslationContext& context)
{
    return context.MakeMExp<MExp_BoolLiteral>(exp->value);
}

expected<MExp*, DiagPtr> TranslateSIntLiteralExpToMExp(SExp_IntLiteral* exp, TranslationContext& context)
{
    return context.MakeMExp<MExp_IntLiteral>(exp->value);
}

expected<NStringExpElement, DiagPtr> TranslateSStringExpElementToRStringExpElement(SStringExpElement* elem, TranslationContext& context)
{
    // TranslationResult<R.StringExpElement> Valid(R.StringExpElement elem) = > TranslationResult.Valid(elem);
    // TranslationResult<R.StringExpElement> Error() = > TranslationResult.Error<R.StringExpElement>();
    // var stringType = context.GetStringType();

    if (auto* expElem = dynamic_cast<SStringExpElement_Exp*>(elem))
    {
        auto eReExp = TranslateSExpToReExp(expElem->exp, /* hintType */ nullptr, context);
        if (!eReExp) return unexpected{move(eReExp).error()};

        auto reExpType = context.GetType(*eReExp);

        // 캐스팅이 필요하다면 
        if (reExpType == context.MakeIntType())
        {   
            auto eNExp = TranslateReExpToMExp(*eReExp, context);
            if (!eNExp) return unexpected{move(eNExp).error()};

            return NLocStringExpElement(
                context.MakeNLoc<MLoc_Temp>(
                    context.MakeMExp<MExp_CallInternalUnaryOperator>(NInternalUnaryOperator::ToString_Int_String, *eNExp)));
        }
        else if (reExpType == context.MakeBoolType())
        {
            auto eNExp = TranslateReExpToMExp(*eReExp, context);
            if (!eNExp) return unexpected{move(eNExp).error()};

            return NLocStringExpElement(
                context.MakeNLoc<MLoc_Temp>(
                    context.MakeMExp<MExp_CallInternalUnaryOperator>(NInternalUnaryOperator::ToString_Bool_String, *eNExp)));
        }
        else if (reExpType == context.MakeStringType())
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

            auto eNLoc = TranslateReExpToMLoc(*eReExp, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
            if (!eNLoc) return unexpected{move(eNLoc).error()};

            return NLocStringExpElement{*eNLoc};
        }
        else
        {
            // TODO: ToString
            return unexpected{MakePtr<Error_StringExp_ExpElementShouldBeBoolOrIntOrString>()};
        }
    }
    else if (auto* textElem = dynamic_cast<SStringExpElement_Text*>(elem))
    {
        return NTextStringExpElement(textElem->text);
    }

    unreachable();
}

expected<MExp_String*, DiagPtr> TranslateSStringExpToNStringExp(SExp_String* exp, TranslationContext& context)
{
    vector<DiagPtr> diags;
    vector<NStringExpElement> builder;
    for(auto& elem : exp->elements)
    {
        auto eRStringExpElem = TranslateSStringExpElementToRStringExpElement(elem, context);

        if (!eRStringExpElem)
        {
            diags.push_back(eRStringExpElem.error());
            continue;
        }
        
        builder.push_back(move(*eRStringExpElem));
    }

    if (!diags.empty())
        return unexpected{MakePtr<AggregateDiag>(move(diags))};

    return context.MakeMExp<MExp_String>(move(builder));
}

// int만 지원한다
expected<MExp*, DiagPtr> TranslateSIntUnaryAssignExpToMExp(SExp* operand, NInternalUnaryAssignOperator op, TranslationContext& context)
{
    // exp를 loc으로 변환하는 일을 하면 안되지만, ref는 풀어야 한다
    // F()++; (x)
    // var& x = i; x++; (o)
    // throws NotLocationException
    
    DesignatedDiagnostic<Error_UnaryAssignOp_AssignableExpressionIsAllowedOnly> designatedDiag;
    auto eNOperand = TranslateSExpToMLoc(operand, /* hintType */ nullptr, /* bWrapExpAsLoc */ false, &designatedDiag, context);
    if (!eNOperand) return unexpected{move(eNOperand).error()};

    // int type 검사, exact match
    if (context.GetType(*eNOperand) != context.MakeIntType())
        return unexpected{MakePtr<Error_UnaryAssignOp_AssignableExpressionIsAllowedOnly>()};

    return context.MakeMExp<MExp_CallInternalUnaryAssignOperator>(op, *eNOperand);
}

expected<MExp*, DiagPtr> TranslateSUnaryOpExpToMExpExceptDeref(SExp_UnaryOp* sExp, TranslationContext& context)
{
    assert(sExp->kind != SUnaryOpKind::Deref);

    // ref 처리
    if (sExp->kind == SUnaryOpKind::Ref)
        return TranslateSExpRefToMExp(sExp->operand, context);

    auto eNOperand = TranslateSExpToMExp(sExp->operand, /*hintType*/ nullptr, context);
    if (!eNOperand) return unexpected{move(eNOperand).error()};

    switch(sExp->kind)
    {
    case SUnaryOpKind::LogicalNot:
    {
        // exact match
        if (context.GetType(*eNOperand) != context.MakeBoolType())
        {   
            return unexpected{MakePtr<Error_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly>()};
        }

        return context.MakeMExp<MExp_CallInternalUnaryOperator>(NInternalUnaryOperator::LogicalNot_Bool_Bool, *eNOperand);
    }

    case SUnaryOpKind::Minus:
    {
        if (context.GetType(*eNOperand) != context.MakeIntType())
        {
            return unexpected{MakePtr<Error_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly>()};
        }

        return context.MakeMExp<MExp_CallInternalUnaryOperator>(NInternalUnaryOperator::UnaryMinus_Int_Int, *eNOperand);
    }

    case SUnaryOpKind::PostfixInc: // e.m++ 등
        return TranslateSIntUnaryAssignExpToMExp(sExp->operand, NInternalUnaryAssignOperator::PostfixInc_Int_Int, context);

    case SUnaryOpKind::PostfixDec:
        return TranslateSIntUnaryAssignExpToMExp(sExp->operand, NInternalUnaryAssignOperator::PostfixDec_Int_Int, context);

    case SUnaryOpKind::PrefixInc:
        return TranslateSIntUnaryAssignExpToMExp(sExp->operand, NInternalUnaryAssignOperator::PrefixInc_Int_Int, context);

    case SUnaryOpKind::PrefixDec:
        return TranslateSIntUnaryAssignExpToMExp(sExp->operand, NInternalUnaryAssignOperator::PrefixDec_Int_Int, context);

    default:
        unreachable();
    }
}

expected<MExp*, DiagPtr> TranslateSAssignBinaryOpExpToMExp(SExp_BinaryOp* exp, TranslationContext& context)
{
    // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다
    DesignatedDiagnostic<Error_BinaryOp_LeftOperandIsNotAssignable> designatedDiag;
    auto eNDestLoc = TranslateSExpToMLoc(exp->operand0, /* hintType */ nullptr, /* bWrapExpAsLoc */ false, &designatedDiag, context);
    if (!eNDestLoc) return unexpected{move(eNDestLoc).error()};

    // 안되는거 체크
    auto* pNDestLoc = *eNDestLoc;
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

    auto nDestLocType = context.GetType(*eNDestLoc);
    auto eNSrcExp = TranslateSExpToMExp(exp->operand1, /*hintType*/ nDestLocType, context);
    if (!eNSrcExp) return unexpected{move(eNSrcExp).error()};

    auto eNWrappedSrcExp = CastMExp(*eNSrcExp, nDestLocType, context);
    if (!eNWrappedSrcExp) return unexpected{move(eNWrappedSrcExp).error()};

    return context.MakeMExp<MExp_Assign>(*eNDestLoc, *eNWrappedSrcExp);
}

expected<MExp*, DiagPtr> TranslateSBinaryOpExpToMExp(SExp_BinaryOp* exp, TranslationContext& context)
{
    // 1. Assign 먼저 처리
    if (exp->kind == SBinaryOpKind::Assign)
    {
        return TranslateSAssignBinaryOpExpToMExp(exp, context);
    }

    auto eOperand0 = TranslateSExpToMExp(exp->operand0, /*hintType*/ nullptr, context);
    if (!eOperand0) return unexpected{move(eOperand0).error()};

    auto eOperand1 = TranslateSExpToMExp(exp->operand1, /*hintType*/ nullptr, context);
    if (!eOperand1) return unexpected{move(eOperand1).error()};

    // 2. NotEqual 처리
    if (exp->kind == SBinaryOpKind::NotEqual)
    {
        const auto& equalInfos = context.GetBinOpInfos(SBinaryOpKind::Equal);
        
        for(auto& info : equalInfos)
        {
            auto castExp0 = CastMExp(*eOperand0, info.operandType0, context);
            if (!castExp0) continue;

            auto castExp1 = CastMExp(*eOperand1, info.operandType1, context);
            if (!castExp1) continue;

            // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
            auto* equalExp = context.MakeMExp<MExp_CallInternalBinaryOperator>(info.rOperator, *castExp0, *castExp1);
            return context.MakeMExp<MExp_CallInternalUnaryOperator>(NInternalUnaryOperator::LogicalNot_Bool_Bool, equalExp);
        }
    }

    // 3. InternalOperator에서 검색            
    auto matchedInfos = context.GetBinOpInfos(exp->kind);
    for(auto& info : matchedInfos)
    {
        auto castExp0 = CastMExp(*eOperand0, info.operandType0, context);
        if (!castExp0) continue;

        auto castExp1 = CastMExp(*eOperand1, info.operandType1, context);
        if (!castExp1) continue;

        // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다

        return context.MakeMExp<MExp_CallInternalBinaryOperator>(info.rOperator, *castExp0, *castExp1);
    }

    // Operator를 찾을 수 없습니다
    return unexpected{MakePtr<Error_BinaryOp_OperatorNotFound>()};
}

expected<MExp*, DiagPtr> TranslateSLambdaExpToMExp(SExp_Lambda* sExp, TranslationContext& context)
{
    // TODO: 리턴 타입과 인자타입은 타입 힌트를 반영해야 한다
    //RType* retType = nullptr;
    
    //auto oLambdaInfo = TranslateLambda(retType, sExp->params, sExp->body, context);

    //if (!oLambdaInfo)
    //    return nullptr;

    // return MakePtr<NLambdaExp>(lambdaInfo.lambda, lambdaInfo.args), context.factory->MakeIn);
    throw NotImplementedException{};
}

expected<MExp*, DiagPtr> TranslateSListExpToMExp(SExp_List* exp, TranslationContext& context)
{
    vector<MExp*> elems;
    elems.reserve(exp->elements.size());

    // TODO: 타입 힌트도 이용해야 할 것 같다
    RType* curElemType = nullptr;

    for(auto& elem : exp->elements)
    {
        auto eNElem = TranslateSExpToMExp(elem, /*hintType*/ nullptr, context);
        if (!eNElem) return unexpected{move(eNElem).error()};

        auto* rElemType = context.GetType(*eNElem);
        elems.push_back(*eNElem);

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

    return context.MakeMExp<MExp_List>(move(elems), curElemType);
}

expected<MExp*, DiagPtr> TranslateSNewExpToMExp(SExp_New* exp, TranslationContext& context) // throws ErrorCodeException
{
    auto eRType = context.TranslateSTypeExpToRType(exp->type);
    if (!eRType) return unexpected{move(eRType).error()};

    if ((*eRType)->GetCustomTypeKind() == RCustomTypeKind::Class)
    {
        return unexpected{MakePtr<Error_NewExp_TypeIsNotClass>()};
    }

    throw NotImplementedException{};
    //var classDecl = classSymbol.GetDecl();

    //var candidates = FuncCandidateSMake&<ClassConstructorDeclSymbol, ClassConstructorSymbol>(
    //    classSymbol, classDecl.GetConstructorCount(), classDecl.GetConstructor, partialTypeArgs: default); // TODO: 일단은 constructor의 typeArgs는 없는 것으로

    //var matchResult = FuncsMatcher.Match(candidates, exp->Args, context);
    //if (matchResult == null)
    //    throw NotImplementedException{}; // 매치에 실패했습니다.

    //var(constructor, args) = matchResult.Value;
    //return Valid(new IR0ExpResult(new R.NewClassExp(constructor, args), new ClassType(classSymbol)));
}

expected<MExp*, DiagPtr> TranslateSCallExpToMExp(SExp_Call* exp, RType* hintType, TranslationContext& context)
{
    auto eImCallable = TranslateSExpToImExp(exp->callable, hintType, context);
    if (!eImCallable) return unexpected{move(eImCallable).error()};

    return TranslateImCallableAndSArgsToMExp(*eImCallable, exp->callable, exp->args, context); // 로깅할때 exp, exp->Callable두개가 다 필요할 수 있다
}

expected<MExp*, DiagPtr> TranslateSBoxExpToMExp(SExp_Box* exp, RType* hintType, TranslationContext& context)
{
    auto* hintBoxPtrType = dynamic_cast<RType_BoxPtr*>(hintType);
    auto innerHintType = hintBoxPtrType ? hintBoxPtrType->innerType : nullptr;

    // hintType전수
    auto eNInnerExp = TranslateSExpToMExp(exp->innerExp, innerHintType, context);
    if (!eNInnerExp) return unexpected{move(eNInnerExp).error()};

    return context.MakeMExp<MExp_Box>(*eNInnerExp);
}

expected<MExp*, DiagPtr> TranslateSIsExpToMExp(SExp_Is* exp, TranslationContext& context)
{
    auto eTarget = TranslateSExpToMExp(exp->exp, /*hintType*/ nullptr, context);
    if (!eTarget) return unexpected{move(eTarget).error()};

    auto targetType = context.GetType(*eTarget);
    auto targetTypeKind = targetType->GetCustomTypeKind();

    auto eTestType = context.TranslateSTypeExpToRType(exp->type);
    if (!eTestType) return unexpected{move(eTestType).error()};

    auto testTypeKind = (*eTestType)->GetCustomTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RCustomTypeKind::Class)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return context.MakeMExp<MExp_ClassIsClass>(*eTarget, *eTestType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return context.MakeMExp<MExp_InterfaceIsClass>(*eTarget, *eTestType);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::Interface)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return context.MakeMExp<MExp_ClassIsInterface>(*eTarget, *eTestType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return context.MakeMExp<MExp_InterfaceIsInterface>(*eTarget, *eTestType);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::EnumElem)
    {
        if (targetTypeKind == RCustomTypeKind::Enum)
            return context.MakeMExp<MExp_EnumIsEnumElem>(*eTarget, *eTestType);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else
        throw NotImplementedException{}; // 에러 처리
}

expected<MExp*, DiagPtr> TranslateSAsExpToMExp(SExp_As* exp, TranslationContext& context)
{
    auto eNTarget = TranslateSExpToMExp(exp->exp, /* hintType */ nullptr, context);
    if (!eNTarget) return unexpected{move(eNTarget).error()};

    auto eNTestType = context.TranslateSTypeExpToRType(exp->type);
    if (!eNTestType) return unexpected{move(eNTestType).error()};

    return context.MakeMExp_As(*eNTarget, *eNTestType);
}

namespace {

// SExp -> MExp
class SExpToMExpTranslator
{
public:
    using ResultType = expected<MExp*, DiagPtr>;

private:
    RType* hintType;
    TranslationContext& context;

public:
    SExpToMExpTranslator(RType* hintType, TranslationContext& context)
        : hintType{hintType}, context{context}
    {
    }

private:
    // S.Exp -> IntermediateExp -> ResolvedExp -> R.Exp
    ResultType HandleDefault(SExp* exp)
    {
        auto eReExp = TranslateSExpToReExp(exp, hintType, context);

        if (!eReExp)
            return unexpected{move(eReExp).error()};

        return TranslateReExpToMExp(*eReExp, context);
    }
    
public:
    ResultType Visit(SExp_Identifier* exp)
    {
        return HandleDefault(exp);
    }

    ResultType Visit(SExp_String* exp)
    {
        return TranslateSStringExpToNStringExp(exp, context);
    }

    ResultType Visit(SExp_IntLiteral* exp)
    {
        return TranslateSIntLiteralExpToMExp(exp, context);
    }

    ResultType Visit(SExp_BoolLiteral* exp)
    {
        return TranslateSBoolLiteralExpToMExp(exp, context);
    }

    ResultType Visit(SExp_NullLiteral* exp)
    {
        return TranslateSNullLiteralExpToMExp(exp, hintType, context);
    }

    ResultType Visit(SExp_BinaryOp* exp)
    {
        return TranslateSBinaryOpExpToMExp(exp, context);
    }

    ResultType Visit(SExp_UnaryOp* exp)
    {
        if (exp->kind == SUnaryOpKind::Deref)
            return HandleDefault(exp);

        return TranslateSUnaryOpExpToMExpExceptDeref(exp, context);
    }

    ResultType Visit(SExp_Call* exp)
    {
        return TranslateSCallExpToMExp(exp, hintType, context);
    }

    ResultType Visit(SExp_Lambda* exp)
    {
        return TranslateSLambdaExpToMExp(exp, context);
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
        return TranslateSListExpToMExp(exp, context);
    }

    ResultType Visit(SExp_New* exp)
    {
        return TranslateSNewExpToMExp(exp, context);
    }

    ResultType Visit(SExp_Box* exp)
    {
        return TranslateSBoxExpToMExp(exp, hintType, context);
    }

    ResultType Visit(SExp_Is* exp)
    {
        return TranslateSIsExpToMExp(exp, context);
    }

    ResultType Visit(SExp_As* exp)
    {
        return TranslateSAsExpToMExp(exp, context);
    }
};

} // namespace 

expected<MExp*, DiagPtr> TranslateSExpToMExp(SExp* exp, RType* hintType, TranslationContext& context)
{
    SExpToMExpTranslator translator{hintType, context};
    return Accept(translator, exp);
}

}