#include "pch.h"
#include "SExpToRExpTranslation.h"

#include <variant>

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Infra/Unreachable.h>
#include <Logging/Logger.h>
#include <Syntax/Syntax.h>
#include <IR0/RType.h>
#include <IR0/RExp.h>
#include <IR0/RLoc.h>
#include <IR0/RTypeFactory.h>

#include "ReExp.h"
#include "ImExp.h"

#include "SExpToRLocTranslation.h"
#include "SExpToReExpTranslation.h"
#include "SExpToImExpTranslation.h"
#include "SExpRefToRExpTranslation.h"

#include "ReExpToRExpTranslation.h"
#include "ReExpToRLocTranslation.h"

#include "ImCallableAndSArgsToRExpTranslation.h"

#include "ScopeContext.h"
#include "DesignatedErrorLogger.h"
#include "Misc.h"
#include "BinOpQueryService.h"
#include "TranslationContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

// Syntax Exp -> IR0 Exp로 바꿔주는 기본적인 코드
// Deref를 적용하지 않는다. 따로 해주어야 한다

RExpPtr TranslateSNullLiteralExpToRExp(SExp_NullLiteral& exp, const RTypePtr& hintType, TranslationContext& context)
{
    if (hintType != nullptr)
    {
        // int? i = null;
        if (dynamic_cast<RType_NullableValue*>(hintType.get()))
            return MakePtr<RExp_NullableValueNullLiteral>(hintType);

        // C? c = null;
        if (dynamic_cast<RType_NullableRef*>(hintType.get()))
            return MakePtr<RExp_NullableRefNullLiteral>(hintType);
    }

    // TODO: if (a == nullptr)도 반영해야 한다
    throw NotImplementedException();
    context.logger->Fatal_Reference_CantMakeReference();
    return nullptr;
}

RExpPtr TranslateSBoolLiteralExpToRExp(SExp_BoolLiteral& exp)
{
    return MakePtr<RExp_BoolLiteral>(exp.value);
}

RExpPtr TranslateSIntLiteralExpToRExp(SExp_IntLiteral& exp)
{
    return MakePtr<RExp_IntLiteral>(exp.value);
}

optional<RStringExpElement> TranslateSStringExpElementToRStringExpElement(const SStringExpElementPtr& elem, TranslationContext& context)
{
    // TranslationResult<R.StringExpElement> Valid(R.StringExpElement elem) = > TranslationResult.Valid(elem);
    // TranslationResult<R.StringExpElement> Error() = > TranslationResult.Error<R.StringExpElement>();
    // var stringType = context.GetStringType();

    context.logger->SetSyntax(elem);

    if (auto* expElem = dynamic_cast<SStringExpElement_Exp*>(elem.get()))
    {
        auto reExp = TranslateSExpToReExp(*expElem->exp, /* hintType */ nullptr, context);
        if (!reExp) return nullopt;

        auto reExpType = reExp->GetType(*context.factory);

        // 캐스팅이 필요하다면 
        if (reExpType == context.factory->MakeIntType())
        {   
            auto rExp = TranslateReExpToRExp(*reExp, context);
            if (!rExp) return nullopt;

            return RLocStringExpElement(
                MakePtr<RLoc_Temp>(
                    MakePtr<RExp_CallInternalUnaryOperator>(RInternalUnaryOperator::ToString_Int_String, std::move(rExp))));
        }
        else if (reExpType == context.factory->MakeBoolType())
        {
            auto rExp = TranslateReExpToRExp(*reExp, context);
            if (!rExp) return nullopt;

            return RLocStringExpElement(
                MakePtr<RLoc_Temp>(
                    MakePtr<RExp_CallInternalUnaryOperator>(RInternalUnaryOperator::ToString_Bool_String, std::move(rExp))));
        }
        else if (reExpType == context.factory->MakeStringType())
        {
            DesignatedErrorLogger designatedErrorLogger(*context.logger, &Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

            auto rLoc = TranslateReExpToRLoc(*reExp, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);
            if (!rLoc) return nullopt;

            return RLocStringExpElement(std::move(rLoc));
        }
        else
        {
            // TODO: ToString
            context.logger->Fatal_StringExp_ExpElementShouldBeBoolOrIntOrString();
            return nullopt;
        }
    }
    else if (auto* textElem = dynamic_cast<SStringExpElement_Text*>(elem.get()))
    {
        return RTextStringExpElement(textElem->text);
    }

    unreachable();
}

std::shared_ptr<RExp_String> TranslateSStringExpToRStringExp(SExp_String& exp, TranslationContext& context)
{
    bool bFatal = false;

    vector<RStringExpElement> builder;
    for(auto& elem : exp.elements)
    {
        auto oRStringExpElem = TranslateSStringExpElementToRStringExpElement(elem, context);

        if (!oRStringExpElem)
        {
            bFatal = true;
            continue;
        }
        
        builder.push_back(std::move(*oRStringExpElem));
    }

    if (bFatal)
        return nullptr;

    return MakePtr<RExp_String>(std::move(builder));
}

// int만 지원한다
RExpPtr TranslateSIntUnaryAssignExpToRExp(SExp& operand, RInternalUnaryAssignOperator op, TranslationContext& context)
{
    // exp를 loc으로 변환하는 일을 하면 안되지만, ref는 풀어야 한다
    // F()++; (x)
    // var& x = i; x++; (o)
    // throws NotLocationException
    
    DesignatedErrorLogger designatedErrorLogger(*context.logger, &Logger::Fatal_UnaryAssignOp_AssignableExpressionIsAllowedOnly);
    auto rOperand = TranslateSExpToRLoc(operand, /* hintType */ nullptr, /* bWrapExpAsLoc */ false, &designatedErrorLogger, context);
    if (!rOperand) return nullptr;

    // int type 검사, exact match
    if (rOperand->GetType(*context.factory) != context.factory->MakeIntType())
    {
        context.logger->Fatal_UnaryAssignOp_AssignableExpressionIsAllowedOnly();
        return nullptr;
    }

    return MakePtr<RExp_CallInternalUnaryAssignOperator>(op, std::move(rOperand));
}

RExpPtr TranslateSUnaryOpExpToRExpExceptDeref(SExp_UnaryOp& sExp, TranslationContext& context)
{
    assert(sExp.kind != SUnaryOpKind::Deref);

    // ref 처리
    if (sExp.kind == SUnaryOpKind::Ref)
    {
        return TranslateSExpRefToRExp(*sExp.operand, context);
    }

    context.logger->SetSyntax(sExp.operand);
    auto rOperand = TranslateSExpToRExp(*sExp.operand, /*hintType*/ nullptr, context);
    if (!rOperand) return nullptr;

    switch(sExp.kind)
    {

    case SUnaryOpKind::LogicalNot:
    {
        // exact match
        if (rOperand->GetType(*context.factory) != context.factory->MakeBoolType())
        {   
            context.logger->Fatal_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly();
            return nullptr;
        }

        return MakePtr<RExp_CallInternalUnaryOperator>(RInternalUnaryOperator::LogicalNot_Bool_Bool, std::move(rOperand));
    }

    case SUnaryOpKind::Minus:
    {
        if (rOperand->GetType(*context.factory) != context.factory->MakeIntType())
        {
            context.logger->Fatal_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly();
            return nullptr;
        }

        return MakePtr<RExp_CallInternalUnaryOperator>(RInternalUnaryOperator::UnaryMinus_Int_Int, std::move(rOperand));
    }

    case SUnaryOpKind::PostfixInc: // e.m++ 등
        return TranslateSIntUnaryAssignExpToRExp(*sExp.operand, RInternalUnaryAssignOperator::PostfixInc_Int_Int, context);

    case SUnaryOpKind::PostfixDec:
        return TranslateSIntUnaryAssignExpToRExp(*sExp.operand, RInternalUnaryAssignOperator::PostfixDec_Int_Int, context);

    case SUnaryOpKind::PrefixInc:
        return TranslateSIntUnaryAssignExpToRExp(*sExp.operand, RInternalUnaryAssignOperator::PrefixInc_Int_Int, context);

    case SUnaryOpKind::PrefixDec:
        return TranslateSIntUnaryAssignExpToRExp(*sExp.operand, RInternalUnaryAssignOperator::PrefixDec_Int_Int, context);

    default:
        unreachable();
    }
}

RExpPtr TranslateSAssignBinaryOpExpToRExp(SExp_BinaryOp& exp, TranslationContext& context)
{
    // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다

    context.logger->SetSyntax(exp.operand0);
    DesignatedErrorLogger designatedErrorLogger(*context.logger, &Logger::Fatal_BinaryOp_LeftOperandIsNotAssignable);
    auto rDestLoc = TranslateSExpToRLoc(*exp.operand0, /* hintType */ nullptr, /* bWrapExpAsLoc */ false, &designatedErrorLogger, context);
    if (!rDestLoc) return nullptr;

    // 안되는거 체크
    auto* pRDestLoc = rDestLoc.get();
    if (dynamic_cast<RLoc_LambdaMemberVar*>(pRDestLoc))
    {
        // int x = 0; var l = () { x = 3; }, TODO: 이거 가능하도록
        context.logger->Fatal_BinaryOp_LeftOperandIsNotAssignable();
        return nullptr;
    }
    else if (dynamic_cast<RLoc_This*>(pRDestLoc))
    {
        context.logger->Fatal_BinaryOp_LeftOperandIsNotAssignable();
        return nullptr;
    }
    else if (dynamic_cast<RLoc_Temp*>(pRDestLoc))
    {
        context.logger->Fatal_BinaryOp_LeftOperandIsNotAssignable();
        return nullptr;
    }

    auto rDestLocType = rDestLoc->GetType(*context.factory);
    context.logger->SetSyntax(exp.operand1);
    auto rSrcExp = TranslateSExpToRExp(*exp.operand1, /*hintType*/ rDestLocType, context);
    if (!rSrcExp) return nullptr;

    auto rWrappedSrcExp = CastRExp(std::move(rSrcExp), rDestLocType, context);
    if (!rWrappedSrcExp) return nullptr;

    return MakePtr<RExp_Assign>(std::move(rDestLoc), std::move(rWrappedSrcExp));
}

RExpPtr TranslateSBinaryOpExpToRExp(SExp_BinaryOp& exp, TranslationContext& context)
{
    // 1. Assign 먼저 처리
    if (exp.kind == SBinaryOpKind::Assign)
    {
        return TranslateSAssignBinaryOpExpToRExp(exp, context);
    }

    auto operand0 = TranslateSExpToRExp(*exp.operand0, /*hintType*/ nullptr, context);
    if (!operand0) return nullptr;

    auto operand1 = TranslateSExpToRExp(*exp.operand1, /*hintType*/ nullptr, context);
    if (!operand1) return nullptr;

    // 2. NotEqual 처리
    if (exp.kind == SBinaryOpKind::NotEqual)
    {
        const auto& equalInfos = context.binOpQueryService->GetInfos(SBinaryOpKind::Equal);
        
        for(auto& info : equalInfos)
        {
            auto castExp0 = TryCastRExp(RExpPtr(operand0), info.operandType0, context);
            if (!castExp0) continue;

            auto castExp1 = TryCastRExp(RExpPtr(operand1), info.operandType1, context);
            if (!castExp1) continue;

            // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
            auto equalExp = MakePtr<RExp_CallInternalBinaryOperator>(info.rOperator, std::move(castExp0), std::move(castExp1));
            return MakePtr<RExp_CallInternalUnaryOperator>(RInternalUnaryOperator::LogicalNot_Bool_Bool, std::move(equalExp));
        }
    }

    // 3. InternalOperator에서 검색            
    auto matchedInfos = context.binOpQueryService->GetInfos(exp.kind);
    for(auto& info : matchedInfos)
    {
        auto castExp0 = TryCastRExp(RExpPtr(operand0), info.operandType0, context);
        if (!castExp0) continue;

        auto castExp1 = TryCastRExp(RExpPtr(operand1), info.operandType1, context);
        if (!castExp1) continue;

        // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다

        return MakePtr<RExp_CallInternalBinaryOperator>(info.rOperator, std::move(castExp0), std::move(castExp1));
    }

    // Operator를 찾을 수 없습니다
    context.logger->Fatal_BinaryOp_OperatorNotFound();
    return nullptr;
}

RExpPtr TranslateSLambdaExpToRExp(SExp_Lambda& sExp, TranslationContext& context)
{
    // TODO: 리턴 타입과 인자타입은 타입 힌트를 반영해야 한다
    //RTypePtr retType = nullptr;
    
    //auto oLambdaInfo = TranslateLambda(retType, sExp.params, sExp.body, context);

    //if (!oLambdaInfo)
    //    return nullptr;

    // return MakePtr<RLambdaExp>(lambdaInfo.lambda, lambdaInfo.args), context.factory->MakeIn);
    static_assert(false);
}

RExpPtr TranslateSListExpToRExp(SExp_List& exp, TranslationContext& context)
{
    vector<RExpPtr> elems;
    elems.reserve(exp.elements.size());

    // TODO: 타입 힌트도 이용해야 할 것 같다
    RTypePtr curElemType = nullptr;

    for(auto& elem : exp.elements)
    {
        auto rElem = TranslateSExpToRExp(*elem, /*hintType*/ nullptr, context);
        if (!rElem) return nullptr;

        auto rElemType = rElem->GetType(*context.factory);
        elems.push_back(std::move(rElem));

        if (curElemType == nullptr)
        {
            curElemType = std::move(rElemType);
            continue;
        }

        if (curElemType != rElemType)
        {
            context.logger->Fatal_ListExp_MismatchBetweenElementTypes();
            return nullptr;
        }
    }

    if (curElemType == nullptr)
    {
        context.logger->Fatal_ListExp_CantInferElementTypeWithEmptyElement();
        return nullptr;
    }

    return MakePtr<RExp_List>(std::move(elems), std::move(curElemType));
}

RExpPtr TranslateSNewExpToRExp(SExp_New& exp, TranslationContext& context) // throws ErrorCodeException
{
    auto rType = context.scopeContext->MakeType(*exp.type, *context.factory);
    if (rType->GetCustomTypeKind() == RCustomTypeKind::Class)
    {
        context.logger->Fatal_NewExp_TypeIsNotClass();
        return nullptr;
    }

    static_assert(false);
    //var classDecl = classSymbol.GetDecl();

    //var candidates = FuncCandidateSMake&<ClassConstructorDeclSymbol, ClassConstructorSymbol>(
    //    classSymbol, classDecl.GetConstructorCount(), classDecl.GetConstructor, partialTypeArgs: default); // TODO: 일단은 constructor의 typeArgs는 없는 것으로

    //var matchResult = FuncsMatcher.Match(candidates, exp.Args, context);
    //if (matchResult == null)
    //    throw NotImplementedException(); // 매치에 실패했습니다.

    //var(constructor, args) = matchResult.Value;
    //return Valid(new IR0ExpResult(new R.NewClassExp(constructor, args), new ClassType(classSymbol)));
}

RExpPtr TranslateSCallExpToRExp(SExp_Call& exp, const RTypePtr& hintType, TranslationContext& context)
{
    auto imCallable = TranslateSExpToImExp(*exp.callable, hintType, context);
    if (!imCallable) return nullptr;

    return TranslateImCallableAndSArgsToRExp(*imCallable, exp.callable, exp.args, context); // 로깅할때 exp, exp.Callable두개가 다 필요할 수 있다
}

RExpPtr TranslateSBoxExpToRExp(SExp_Box& exp, const RTypePtr& hintType, TranslationContext& context)
{
    auto* hintBoxPtrType = dynamic_cast<RType_BoxPtr*>(hintType.get());
    auto innerHintType = hintBoxPtrType ? hintBoxPtrType->innerType : nullptr;

    // hintType전수
    auto rInnerExp = TranslateSExpToRExp(*exp.innerExp, innerHintType, context);
    if (!rInnerExp) return nullptr;

    return MakePtr<RExp_Box>(std::move(rInnerExp));
}

RExpPtr TranslateSIsExpToRExp(SExp_Is& exp, TranslationContext& context)
{
    auto target = TranslateSExpToRExp(*exp.exp, /*hintType*/ nullptr, context);
    if (!target) return nullptr;

    auto targetType = target->GetType(*context.factory);
    auto targetTypeKind = targetType->GetCustomTypeKind();

    auto testType = context.scopeContext->MakeType(*exp.type, *context.factory);
    auto testTypeKind = testType->GetCustomTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RCustomTypeKind::Class)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<RExp_ClassIsClass>(std::move(target), testType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<RExp_InterfaceIsClass>(std::move(target), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::Interface)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<RExp_ClassIsInterface>(std::move(target), testType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<RExp_InterfaceIsInterface>(std::move(target), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::EnumElem)
    {
        if (targetTypeKind == RCustomTypeKind::Enum)
            return MakePtr<RExp_EnumIsEnumElem>(std::move(target), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else
        throw NotImplementedException(); // 에러 처리
}

RExpPtr TranslateSAsExpToRExp(SExp_As& exp, TranslationContext& context)
{
    auto rTarget = TranslateSExpToRExp(*exp.exp, /* hintType */ nullptr, context);
    if (!rTarget) return nullptr;    

    auto rTestType = context.scopeContext->MakeType(*exp.type, *context.factory);

    return MakeRExp_As(std::move(rTarget), rTestType, *context.factory);
}

namespace {

// S.Exp -> R.Exp
class SExpToRExpTranslator : public SExpVisitor
{
    RTypePtr hintType;
    RExpPtr* result;

    TranslationContext& context;

public:
    SExpToRExpTranslator(const RTypePtr& hintType, RExpPtr* result, TranslationContext& context)
        : hintType(hintType), result(result), context(context)
    {
    }

    // S.Exp -> IntermediateExp -> ResolvedExp -> R.Exp
    void HandleDefault(SExp& exp)
    {
        auto reExp = TranslateSExpToReExp(exp, hintType, context);

        if (reExp)
            *result = TranslateReExpToRExp(*reExp, context);
        else
            *result = nullptr;
    }

    void Visit(SExp_Identifier& exp) override
    {
        return HandleDefault(exp);
    }

    void Visit(SExp_String& exp) override
    {
        *result = TranslateSStringExpToRStringExp(exp, context);
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        *result = TranslateSIntLiteralExpToRExp(exp);
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        *result = TranslateSBoolLiteralExpToRExp(exp);
    }

    void Visit(SExp_NullLiteral& exp) override
    {
        *result = TranslateSNullLiteralExpToRExp(exp, hintType, context);
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        *result = TranslateSBinaryOpExpToRExp(exp, context);
    }

    void Visit(SExp_UnaryOp& exp) override
    {
        if (exp.kind == SUnaryOpKind::Deref)
            return HandleDefault(exp);

        *result = TranslateSUnaryOpExpToRExpExceptDeref(exp, context);
    }

    void Visit(SExp_Call& exp) override
    {
        *result = TranslateSCallExpToRExp(exp, hintType, context);
    }

    void Visit(SExp_Lambda& exp) override
    {
        // context.logger->SetSyntax(syntax);
        *result = TranslateSLambdaExpToRExp(exp, context);
    }

    void Visit(SExp_Indexer& exp) override
    {
        return HandleDefault(exp);
    }

    void Visit(SExp_Member& exp) override
    {
        return HandleDefault(exp);
    }

    void Visit(SExp_IndirectMember& exp) override
    {
        static_assert(false);
    }

    void Visit(SExp_List& exp) override
    {
        *result = TranslateSListExpToRExp(exp, context);
    }

    void Visit(SExp_New& exp) override
    {
        *result = TranslateSNewExpToRExp(exp, context);
    }

    void Visit(SExp_Box& exp) override
    {
        *result = TranslateSBoxExpToRExp(exp, hintType, context);
    }

    void Visit(SExp_Is& exp) override
    {
        *result = TranslateSIsExpToRExp(exp, context);
    }

    void Visit(SExp_As& exp) override
    {
        *result = TranslateSAsExpToRExp(exp, context);
    }
};

} // namespace 

RExpPtr TranslateSExpToRExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context)
{
    RExpPtr rExp;
    SExpToRExpTranslator translator(hintType, &rExp, context);
    exp.Accept(translator);
    return rExp;
}

}