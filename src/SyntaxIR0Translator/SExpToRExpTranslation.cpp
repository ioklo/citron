#include "pch.h"
#include "SExpToRExpTranslation.h"

#include <variant>

#include <Infra/Ptr.h>
#include <Infra/NotImplementedException.h>
#include <Infra/Unreachable.h>
#include <Logging/Logger.h>
#include <Syntax/Syntax.h>
#include <IR0/RType.h>
#include <IR0/RExp.h>
#include <IR0/RLoc.h>
#include <IR0/RTypeFactory.h>

#include "ReExp.h"

#include "SExpToRLocTranslation.h"
#include "SExpToReExpTranslation.h"
#include "SExpToImExpTranslation.h"
#include "SExpRefToRExpTranslation.h"

#include "ReExpToRExpTranslation.h"
#include "ReExpToRLocTranslation.h"

#include "ImExpAndSArgsToRExpBinding.h"

#include "ScopeContext.h"
#include "NotLocationErrorLogger.h"
#include "Misc.h"
#include "BinOpQueryService.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

// Syntax Exp -> IR0 Exp로 바꿔주는 기본적인 코드
// Deref를 적용하지 않는다. 따로 해주어야 한다

RExpPtr TranslateSNullLiteralExpToRExp(SNullLiteralExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger)
{
    if (hintType != nullptr)
    {
        // int? i = null;
        if (dynamic_cast<RNullableValueType*>(hintType.get()))
            return MakePtr<RNullableValueNullLiteralExp>(hintType);

        // C? c = null;
        if (dynamic_cast<RNullableRefType*>(hintType.get()))
            return MakePtr<RNullableRefNullLiteralExp>(hintType);
    }

    // TODO: if (a == nullptr)도 반영해야 한다
    throw NotImplementedException();
    logger->Fatal_CantMakeReference();
    return nullptr;
}

RExpPtr TranslateSBoolLiteralExpToRExp(SBoolLiteralExp& exp)
{
    return MakePtr<RBoolLiteralExp>(exp.value);
}

RExpPtr TranslateSIntLiteralExpToRExp(SIntLiteralExp& exp)
{
    return MakePtr<RIntLiteralExp>(exp.value);
}

optional<RStringExpElement> TranslateSStringExpElementToRStringExpElement(const SStringExpElementPtr& elem, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    // TranslationResult<R.StringExpElement> Valid(R.StringExpElement elem) = > TranslationResult.Valid(elem);
    // TranslationResult<R.StringExpElement> Error() = > TranslationResult.Error<R.StringExpElement>();
    // var stringType = context.GetStringType();

    logger->SetSyntax(elem);

    if (auto* expElem = dynamic_cast<SExpStringExpElement*>(elem.get()))
    {
        auto reExp = TranslateSExpToReExp(*expElem->exp, /* hintType */ nullptr, context);
        if (!reExp) return nullopt;

        auto reExpType = reExp->GetType();

        // 캐스팅이 필요하다면 
        if (reExpType == factory.MakeIntType())
        {   
            auto rExp = TranslateReExpToRExp(*reExp, context, logger, factory);
            if (!rExp) return nullopt;

            return RLocStringExpElement(
                MakePtr<RTempLoc>(
                    MakePtr<RCallInternalUnaryOperatorExp>(RInternalUnaryOperator::ToString_Int_String, std::move(rExp))));
        }
        else if (reExpType == factory.MakeBoolType())
        {
            auto rExp = TranslateReExpToRExp(*reExp, context, logger, factory);
            if (!rExp) return nullopt;

            return RLocStringExpElement(
                MakePtr<RTempLoc>(
                    MakePtr<RCallInternalUnaryOperatorExp>(RInternalUnaryOperator::ToString_Bool_String, std::move(rExp))));
        }
        else if (reExpType == factory.MakeStringType())
        {
            ExpressionIsNotLocationErrorLogger notLocationErrorLogger(logger);

            auto rLoc = TranslateReExpToRLoc(*reExp, /*bWrapExpAsLoc*/ true, &notLocationErrorLogger, context, logger, factory);
            if (!rLoc) return nullopt;

            return RLocStringExpElement(std::move(rLoc));
        }
        else
        {
            // TODO: ToString
            logger->Fatal_ExpElementShouldBeBoolOrIntOrString();
            return nullopt;
        }
    }
    else if (auto* textElem = dynamic_cast<STextStringExpElement*>(elem.get()))
    {
        return RTextStringExpElement(textElem->text);
    }

    unreachable();
}

std::shared_ptr<RStringExp> TranslateSStringExpToRStringExp(SStringExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    bool bFatal = false;

    vector<RStringExpElement> builder;
    for(auto& elem : exp.elements)
    {
        auto oRStringExpElem = TranslateSStringExpElementToRStringExpElement(elem, context, logger, factory);

        if (!oRStringExpElem)
        {
            bFatal = true;
            continue;
        }
        
        builder.push_back(std::move(*oRStringExpElem));
    }

    if (bFatal)
        return nullptr;

    return MakePtr<RStringExp>(std::move(builder));
}

// int만 지원한다
RExpPtr TranslateSIntUnaryAssignExpToRExp(SExp& operand, RInternalUnaryAssignOperator op, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    // exp를 loc으로 변환하는 일을 하면 안되지만, ref는 풀어야 한다
    // F()++; (x)
    // var& x = i; x++; (o)
    // throws NotLocationException
    
    NotLocationErrorLogger notLocationErrorLogger(logger, &Logger::Fatal_AssignableExpressionIsAllowedOnly);
    auto rOperand = TranslateSExpToRLoc(operand, /* hintType */ nullptr, /* bWrapExpAsLoc */ false, &notLocationErrorLogger, context, logger);
    if (!rOperand) return nullptr;

    // int type 검사, exact match
    if (rOperand->GetType(factory) != factory.MakeIntType())
    {
        logger->Fatal_AssignableExpressionIsAllowedOnly();
        return nullptr;
    }

    return MakePtr<RCallInternalUnaryAssignOperatorExp>(op, std::move(rOperand));
}

RExpPtr TranslateSUnaryOpExpToRExpExceptDeref(SUnaryOpExp& sExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    assert(sExp.kind != SUnaryOpKind::Deref);

    // ref 처리
    if (sExp.kind == SUnaryOpKind::Ref)
    {
        return TranslateSExpRefToRExp(*sExp.operand, context);
    }

    logger->SetSyntax(sExp.operand);
    auto rOperand = TranslateSExpToRExp(*sExp.operand, /*hintType*/ nullptr, context, logger, factory);
    if (!rOperand) return nullptr;

    switch(sExp.kind)
    {

    case SUnaryOpKind::LogicalNot:
    {
        // exact match
        if (rOperand->GetType(factory) != factory.MakeBoolType())
        {   
            logger->Fatal_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly();
            return nullptr;
        }

        return MakePtr<RCallInternalUnaryOperatorExp>(RInternalUnaryOperator::LogicalNot_Bool_Bool, std::move(rOperand));
    }

    case SUnaryOpKind::Minus:
    {
        if (rOperand->GetType(factory) != factory.MakeIntType())
        {
            logger->Fatal_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly();
            return nullptr;
        }

        return MakePtr<RCallInternalUnaryOperatorExp>(RInternalUnaryOperator::UnaryMinus_Int_Int, std::move(rOperand));
    }

    case SUnaryOpKind::PostfixInc: // e.m++ 등
        return TranslateSIntUnaryAssignExpToRExp(*sExp.operand, RInternalUnaryAssignOperator::PostfixInc_Int_Int, context, logger, factory);

    case SUnaryOpKind::PostfixDec:
        return TranslateSIntUnaryAssignExpToRExp(*sExp.operand, RInternalUnaryAssignOperator::PostfixDec_Int_Int, context, logger, factory);

    case SUnaryOpKind::PrefixInc:
        return TranslateSIntUnaryAssignExpToRExp(*sExp.operand, RInternalUnaryAssignOperator::PrefixInc_Int_Int, context, logger, factory);

    case SUnaryOpKind::PrefixDec:
        return TranslateSIntUnaryAssignExpToRExp(*sExp.operand, RInternalUnaryAssignOperator::PrefixDec_Int_Int, context, logger, factory);

    default:
        unreachable();
    }
}

RExpPtr TranslateSAssignBinaryOpExpToRExp(SBinaryOpExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다

    logger->SetSyntax(exp.operand0);
    NotLocationErrorLogger notLocationErrorLogger(logger, &Logger::Fatal_LeftOperandIsNotAssignable);
    auto rDestLoc = TranslateSExpToRLoc(*exp.operand0, /* hintType */ nullptr, /* bWrapExpAsLoc */ false, &notLocationErrorLogger, context, logger);
    if (!rDestLoc) return nullptr;

    // 안되는거 체크
    auto* pRDestLoc = rDestLoc.get();
    if (dynamic_cast<RLambdaMemberVarLoc*>(pRDestLoc))
    {
        // int x = 0; var l = () { x = 3; }, TODO: 이거 가능하도록
        logger->Fatal_LeftOperandIsNotAssignable();
        return nullptr;
    }
    else if (dynamic_cast<RThisLoc*>(pRDestLoc))
    {
        logger->Fatal_LeftOperandIsNotAssignable();
        return nullptr;
    }
    else if (dynamic_cast<RTempLoc*>(pRDestLoc))
    {
        logger->Fatal_LeftOperandIsNotAssignable();
        return nullptr;
    }

    auto rDestLocType = rDestLoc->GetType(factory);
    logger->SetSyntax(exp.operand1);
    auto rSrcExp = TranslateSExpToRExp(*exp.operand1, /*hintType*/ rDestLocType, context, logger, factory);
    if (!rSrcExp) return nullptr;

    auto rWrappedSrcExp = CastRExp(rSrcExp, rDestLocType, context, logger);
    if (!rWrappedSrcExp) return nullptr;

    return MakePtr<RAssignExp>(std::move(rDestLoc), std::move(rWrappedSrcExp));
}

RExpPtr TranslateSBinaryOpExpToRExp(SBinaryOpExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    // 1. Assign 먼저 처리
    if (exp.kind == SBinaryOpKind::Assign)
    {
        return TranslateSAssignBinaryOpExpToRExp(exp, context, logger, factory);
    }

    auto operand0 = TranslateSExpToRExp(*exp.operand0, /*hintType*/ nullptr, context, logger, factory);
    if (!operand0) return nullptr;

    auto operand1 = TranslateSExpToRExp(*exp.operand1, /*hintType*/ nullptr, context, logger, factory);
    if (!operand1) return nullptr;

    // 2. NotEqual 처리
    if (exp.kind == SBinaryOpKind::NotEqual)
    {
        const auto& equalInfos = context->GetBinOpInfos(SBinaryOpKind::Equal);
        
        for(auto& info : equalInfos)
        {
            auto castExp0 = TryCastRExp(operand0, info.operandType0, context);
            if (!castExp0) continue;

            auto castExp1 = TryCastRExp(operand1, info.operandType1, context);
            if (!castExp1) continue;

            // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
            auto equalExp = MakePtr<RCallInternalBinaryOperatorExp>(info.rOperator, std::move(castExp0), std::move(castExp1));
            return MakePtr<RCallInternalUnaryOperatorExp>(RInternalUnaryOperator::LogicalNot_Bool_Bool, std::move(equalExp));
        }
    }

    // 3. InternalOperator에서 검색            
    auto matchedInfos = context->GetBinOpInfos(exp.kind);
    for(auto& info : matchedInfos)
    {
        auto castExp0 = TryCastRExp(operand0, info.operandType0, context);
        if (!castExp0) continue;

        auto castExp1 = TryCastRExp(operand1, info.operandType1, context);
        if (!castExp1) continue;

        // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다

        return MakePtr<RCallInternalBinaryOperatorExp>(info.rOperator, std::move(castExp0), std::move(castExp1));
    }

    // Operator를 찾을 수 없습니다
    logger->Fatal_OperatorNotFound();
    return nullptr;
}

RExpPtr TranslateSLambdaExpToRExp(SLambdaExp& sExp, const LoggerPtr& logger)
{
    // TODO: 리턴 타입과 인자타입은 타입 힌트를 반영해야 한다
    //RTypePtr retType = nullptr;
    
    //auto oLambdaInfo = TranslateLambda(retType, sExp.params, sExp.body, context, logger);

    //if (!oLambdaInfo)
    //    return nullptr;

    // return MakePtr<RLambdaExp>(lambdaInfo.lambda, lambdaInfo.args), factory.MakeIn);
    static_assert(false);
}

RExpPtr TranslateSListExpToRExp(SListExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    vector<RExpPtr> elems;
    elems.reserve(exp.elements.size());

    // TODO: 타입 힌트도 이용해야 할 것 같다
    RTypePtr curElemType = nullptr;

    for(auto& elem : exp.elements)
    {
        auto rElem = TranslateSExpToRExp(*elem, /*hintType*/ nullptr, context, logger, factory);
        if (!rElem) return nullptr;

        auto rElemType = rElem->GetType(factory);
        elems.push_back(std::move(rElem));

        if (curElemType == nullptr)
        {
            curElemType = std::move(rElemType);
            continue;
        }

        if (curElemType != rElemType)
        {
            logger->Fatal_MismatchBetweenElementTypes();
            return nullptr;
        }
    }

    if (curElemType == nullptr)
    {
        logger->Fatal_CantInferElementTypeWithEmptyElement();
        return nullptr;
    }

    return MakePtr<RListExp>(std::move(elems), std::move(curElemType));
}

RExpPtr TranslateSNewExpToRExp(SNewExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory) // throws ErrorCodeException
{
    auto rType = context->MakeType(*exp.type);
    if (rType->GetCustomTypeKind() == RCustomTypeKind::Class)
    {
        logger->Fatal_TypeIsNotClass();
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

RExpPtr TranslateSCallExpToRExp(SCallExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger)
{
    auto imCallable = TranslateSExpToImExp(*exp.callable, hintType, context);
    if (!imCallable) return nullptr;

    return BindImExpAndSArgsToRExp(*imCallable, exp.args, context, logger); // 로깅할때 exp, exp.Callable두개가 다 필요할 수 있다
    //return CallableAndArgsBinder.Bind(callable, exp.Args, context, exp, exp.Callable);
}

RExpPtr TranslateSBoxExpToRExp(SBoxExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    auto* hintBoxPtrType = dynamic_cast<RBoxPtrType*>(hintType.get());
    auto innerHintType = hintBoxPtrType ? hintBoxPtrType->innerType : nullptr;

    // hintType전수
    auto rInnerExp = TranslateSExpToRExp(*exp.innerExp, innerHintType, context, logger, factory);
    if (!rInnerExp) return nullptr;

    return MakePtr<RBoxExp>(std::move(rInnerExp));
}

RExpPtr TranslateSIsExpToRExp(SIsExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    auto target = TranslateSExpToRExp(*exp.exp, /*hintType*/ nullptr, context, logger, factory);
    if (!target) return nullptr;

    auto targetType = target->GetType(factory);
    auto targetTypeKind = targetType->GetCustomTypeKind();

    auto testType = context->MakeType(*exp.type);
    auto testTypeKind = testType->GetCustomTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RCustomTypeKind::Class)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<RClassIsClassExp>(std::move(target), testType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<RInterfaceIsClassExp>(std::move(target), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::Interface)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<RClassIsInterfaceExp>(std::move(target), testType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<RInterfaceIsInterfaceExp>(std::move(target), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::EnumElem)
    {
        if (targetTypeKind == RCustomTypeKind::Enum)
            return MakePtr<REnumIsEnumElemExp>(std::move(target), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else
        throw NotImplementedException(); // 에러 처리
}

RExpPtr TranslateSAsExpToRExp(SAsExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    auto rTarget = TranslateSExpToRExp(*exp.exp, /* hintType */ nullptr, context, logger, factory);
    if (!rTarget) return nullptr;    

    auto rTestType = context->MakeType(*exp.type);

    return MakeRAsExp(rTarget->GetType(factory), rTestType, std::move(rTarget));
}

// S.Exp -> R.Exp
class SExpToRExpTranslator : public SExpVisitor
{
    RTypePtr hintType;
    RExpPtr* result;

    ScopeContextPtr context;
    LoggerPtr logger;
    RTypeFactory& factory;

public:
    SExpToRExpTranslator(const RTypePtr& hintType, RExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
        : hintType(hintType), result(result), context(context), logger(logger), factory(factory)
    {
    }

    // S.Exp -> IntermediateExp -> ResolvedExp -> R.Exp
    void HandleDefault(SExp& exp)
    {
        auto reExp = TranslateSExpToReExp(exp, hintType, context);

        if (reExp)
            *result = TranslateReExpToRExp(*reExp, context, logger, factory);
        else
            *result = nullptr;
    }

    void Visit(SIdentifierExp& exp) override 
    {
        return HandleDefault(exp);
    }

    void Visit(SStringExp& exp) override 
    {
        *result = TranslateSStringExpToRStringExp(exp, context, logger, factory);
    }

    void Visit(SIntLiteralExp& exp) override 
    {
        *result = TranslateSIntLiteralExpToRExp(exp);
    }

    void Visit(SBoolLiteralExp& exp) override 
    {
        *result = TranslateSBoolLiteralExpToRExp(exp);
    }

    void Visit(SNullLiteralExp& exp) override 
    {
        *result = TranslateSNullLiteralExpToRExp(exp, hintType, context, logger);
    }

    void Visit(SBinaryOpExp& exp) override 
    {
        *result = TranslateSBinaryOpExpToRExp(exp, context, logger, factory);
    }

    void Visit(SUnaryOpExp& exp) override 
    {
        if (exp.kind == SUnaryOpKind::Deref)
            return HandleDefault(exp);

        *result = TranslateSUnaryOpExpToRExpExceptDeref(exp, context, logger, factory);
    }

    void Visit(SCallExp& exp) override 
    {
        *result = TranslateSCallExpToRExp(exp, hintType, context, logger);
    }

    void Visit(SLambdaExp& exp) override 
    {
        // logger->SetSyntax(syntax);
        *result = TranslateSLambdaExpToRExp(exp, logger);
    }

    void Visit(SIndexerExp& exp) override 
    {
        return HandleDefault(exp);
    }

    void Visit(SMemberExp& exp) override 
    {
        return HandleDefault(exp);
    }

    void Visit(SIndirectMemberExp& exp) override 
    {
        static_assert(false);
    }

    void Visit(SListExp& exp) override 
    {
        *result = TranslateSListExpToRExp(exp, context, logger, factory);
    }

    void Visit(SNewExp& exp) override 
    {
        *result = TranslateSNewExpToRExp(exp, context, logger, factory);
    }

    void Visit(SBoxExp& exp) override 
    {
        *result = TranslateSBoxExpToRExp(exp, hintType, context, logger, factory);
    }

    void Visit(SIsExp& exp) override 
    {
        *result = TranslateSIsExpToRExp(exp, context, logger, factory);
    }

    void Visit(SAsExp& exp) override 
    {
        *result = TranslateSAsExpToRExp(exp, context, logger, factory);
    }
};

RExpPtr TranslateSExpToRExp(SExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    RExpPtr rExp;
    SExpToRExpTranslator translator(hintType, &rExp, context, logger, factory);
    exp.Accept(translator);
    return rExp;
}

}