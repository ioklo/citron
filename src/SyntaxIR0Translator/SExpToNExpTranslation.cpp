#include "pch.h"
#include "SExpToNExpTranslation.h"

#include <variant>

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Infra/Unreachable.h>
#include <Logging/Logger.h>
#include <Syntax/Syntax.h>
#include <IR0/RType.h>
#include <IR0/NExp.h>
#include <IR0/NLoc.h>
#include <IR0/RTypeFactory.h>

#include "ReExp.h"
#include "ImExp.h"

#include "SExpToNLocTranslation.h"
#include "SExpToReExpTranslation.h"
#include "SExpToImExpTranslation.h"
#include "SExpRefToNExpTranslation.h"

#include "ReExpToNExpTranslation.h"
#include "ReExpToNLocTranslation.h"

#include "ImCallableAndSArgsToNExpTranslation.h"

#include "ScopeContext.h"
#include "DesignatedErrorLogger.h"
#include "Misc.h"
#include "BinOpQueryService.h"
#include "TranslationContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

// Syntax Exp -> IR0 Exp로 바꿔주는 기본적인 코드
// Deref를 적용하지 않는다. 따로 해주어야 한다

NExpPtr TranslateSNullLiteralExpToNExp(SExp_NullLiteral& exp, const RTypePtr& hintType, TranslationContext& context)
{
    if (hintType != nullptr)
    {
        // int? i = null;
        if (dynamic_cast<RType_NullableValue*>(hintType.get()))
            return MakePtr<NExp_NullableValueNullLiteral>(hintType);

        // C? c = null;
        if (dynamic_cast<RType_NullableRef*>(hintType.get()))
            return MakePtr<NExp_NullableRefNullLiteral>(hintType);
    }

    // TODO: if (a == nullptr)도 반영해야 한다
    throw NotImplementedException();
    context.Log(&Logger::Fatal_Reference_CantMakeReference);
    return nullptr;
}

NExpPtr TranslateSBoolLiteralExpToNExp(SExp_BoolLiteral& exp)
{
    return MakePtr<NExp_BoolLiteral>(exp.value);
}

NExpPtr TranslateSIntLiteralExpToNExp(SExp_IntLiteral& exp)
{
    return MakePtr<NExp_IntLiteral>(exp.value);
}

optional<RStringExpElement> TranslateSStringExpElementToRStringExpElement(const SStringExpElementPtr& elem, TranslationContext& context)
{
    // TranslationResult<R.StringExpElement> Valid(R.StringExpElement elem) = > TranslationResult.Valid(elem);
    // TranslationResult<R.StringExpElement> Error() = > TranslationResult.Error<R.StringExpElement>();
    // var stringType = context.GetStringType();

    context.SetSyntax(elem);

    if (auto* expElem = dynamic_cast<SStringExpElement_Exp*>(elem.get()))
    {
        auto reExp = TranslateSExpToReExp(*expElem->exp, /* hintType */ nullptr, context);
        if (!reExp) return nullopt;

        auto reExpType = context.GetType(*reExp);

        // 캐스팅이 필요하다면 
        if (reExpType == context.MakeIntType())
        {   
            auto nExp = TranslateReExpToNExp(*reExp, context);
            if (!nExp) return nullopt;

            return RLocStringExpElement(
                MakePtr<NLoc_Temp>(
                    MakePtr<NExp_CallInternalUnaryOperator>(RInternalUnaryOperator::ToString_Int_String, std::move(nExp))));
        }
        else if (reExpType == context.MakeBoolType())
        {
            auto nExp = TranslateReExpToNExp(*reExp, context);
            if (!nExp) return nullopt;

            return RLocStringExpElement(
                MakePtr<NLoc_Temp>(
                    MakePtr<NExp_CallInternalUnaryOperator>(RInternalUnaryOperator::ToString_Bool_String, std::move(nExp))));
        }
        else if (reExpType == context.MakeStringType())
        {
            auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

            auto nLoc = TranslateReExpToNLoc(*reExp, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);
            if (!nLoc) return nullopt;

            return RLocStringExpElement(std::move(nLoc));
        }
        else
        {
            // TODO: ToString
            context.Log(&Logger::Fatal_StringExp_ExpElementShouldBeBoolOrIntOrString);
            return nullopt;
        }
    }
    else if (auto* textElem = dynamic_cast<SStringExpElement_Text*>(elem.get()))
    {
        return RTextStringExpElement(textElem->text);
    }

    unreachable();
}

std::shared_ptr<NExp_String> TranslateSStringExpToNStringExp(SExp_String& exp, TranslationContext& context)
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

    return MakePtr<NExp_String>(std::move(builder));
}

// int만 지원한다
NExpPtr TranslateSIntUnaryAssignExpToNExp(SExp& operand, RInternalUnaryAssignOperator op, TranslationContext& context)
{
    // exp를 loc으로 변환하는 일을 하면 안되지만, ref는 풀어야 한다
    // F()++; (x)
    // var& x = i; x++; (o)
    // throws NotLocationException
    
    auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_UnaryAssignOp_AssignableExpressionIsAllowedOnly);
    auto nOperand = TranslateSExpToNLoc(operand, /* hintType */ nullptr, /* bWrapExpAsLoc */ false, &designatedErrorLogger, context);
    if (!nOperand) return nullptr;

    // int type 검사, exact match
    if (context.GetType(*nOperand) != context.MakeIntType())
    {
        context.Log(&Logger::Fatal_UnaryAssignOp_AssignableExpressionIsAllowedOnly);
        return nullptr;
    }

    return MakePtr<NExp_CallInternalUnaryAssignOperator>(op, std::move(nOperand));
}

NExpPtr TranslateSUnaryOpExpToNExpExceptDeref(SExp_UnaryOp& sExp, TranslationContext& context)
{
    assert(sExp.kind != SUnaryOpKind::Deref);

    // ref 처리
    if (sExp.kind == SUnaryOpKind::Ref)
    {
        return TranslateSExpRefToNExp(*sExp.operand, context);
    }

    context.SetSyntax(sExp.operand);
    auto nOperand = TranslateSExpToNExp(*sExp.operand, /*hintType*/ nullptr, context);
    if (!nOperand) return nullptr;

    switch(sExp.kind)
    {

    case SUnaryOpKind::LogicalNot:
    {
        // exact match
        if (context.GetType(*nOperand) != context.MakeBoolType())
        {   
            context.Log(&Logger::Fatal_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly);
            return nullptr;
        }

        return MakePtr<NExp_CallInternalUnaryOperator>(RInternalUnaryOperator::LogicalNot_Bool_Bool, std::move(nOperand));
    }

    case SUnaryOpKind::Minus:
    {
        if (context.GetType(*nOperand) != context.MakeIntType())
        {
            context.Log(&Logger::Fatal_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly);
            return nullptr;
        }

        return MakePtr<NExp_CallInternalUnaryOperator>(RInternalUnaryOperator::UnaryMinus_Int_Int, std::move(nOperand));
    }

    case SUnaryOpKind::PostfixInc: // e.m++ 등
        return TranslateSIntUnaryAssignExpToNExp(*sExp.operand, RInternalUnaryAssignOperator::PostfixInc_Int_Int, context);

    case SUnaryOpKind::PostfixDec:
        return TranslateSIntUnaryAssignExpToNExp(*sExp.operand, RInternalUnaryAssignOperator::PostfixDec_Int_Int, context);

    case SUnaryOpKind::PrefixInc:
        return TranslateSIntUnaryAssignExpToNExp(*sExp.operand, RInternalUnaryAssignOperator::PrefixInc_Int_Int, context);

    case SUnaryOpKind::PrefixDec:
        return TranslateSIntUnaryAssignExpToNExp(*sExp.operand, RInternalUnaryAssignOperator::PrefixDec_Int_Int, context);

    default:
        unreachable();
    }
}

NExpPtr TranslateSAssignBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context)
{
    // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다

    context.SetSyntax(exp.operand0);
    auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_BinaryOp_LeftOperandIsNotAssignable);
    auto nDestLoc = TranslateSExpToNLoc(*exp.operand0, /* hintType */ nullptr, /* bWrapExpAsLoc */ false, &designatedErrorLogger, context);
    if (!nDestLoc) return nullptr;

    // 안되는거 체크
    auto* pRDestLoc = nDestLoc.get();
    if (dynamic_cast<NLoc_LambdaMemberVar*>(pRDestLoc))
    {
        // int x = 0; var l = () { x = 3; }, TODO: 이거 가능하도록
        context.Log(&Logger::Fatal_BinaryOp_LeftOperandIsNotAssignable);
        return nullptr;
    }
    else if (dynamic_cast<NLoc_This*>(pRDestLoc))
    {
        context.Log(&Logger::Fatal_BinaryOp_LeftOperandIsNotAssignable);
        return nullptr;
    }
    else if (dynamic_cast<NLoc_Temp*>(pRDestLoc))
    {
        context.Log(&Logger::Fatal_BinaryOp_LeftOperandIsNotAssignable);
        return nullptr;
    }

    auto rDestLocType = context.GetType(*nDestLoc);
    context.SetSyntax(exp.operand1);
    auto nSrcExp = TranslateSExpToNExp(*exp.operand1, /*hintType*/ rDestLocType, context);
    if (!nSrcExp) return nullptr;

    auto nWrappedSrcExp = CastNExp(std::move(nSrcExp), rDestLocType, context);
    if (!nWrappedSrcExp) return nullptr;

    return MakePtr<NExp_Assign>(std::move(nDestLoc), std::move(nWrappedSrcExp));
}

NExpPtr TranslateSBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context)
{
    // 1. Assign 먼저 처리
    if (exp.kind == SBinaryOpKind::Assign)
    {
        return TranslateSAssignBinaryOpExpToNExp(exp, context);
    }

    auto operand0 = TranslateSExpToNExp(*exp.operand0, /*hintType*/ nullptr, context);
    if (!operand0) return nullptr;

    auto operand1 = TranslateSExpToNExp(*exp.operand1, /*hintType*/ nullptr, context);
    if (!operand1) return nullptr;

    // 2. NotEqual 처리
    if (exp.kind == SBinaryOpKind::NotEqual)
    {
        const auto& equalInfos = context.GetBinOpInfos(SBinaryOpKind::Equal);
        
        for(auto& info : equalInfos)
        {
            auto castExp0 = TryCastRExp(NExpPtr(operand0), info.operandType0, context);
            if (!castExp0) continue;

            auto castExp1 = TryCastRExp(NExpPtr(operand1), info.operandType1, context);
            if (!castExp1) continue;

            // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
            auto equalExp = MakePtr<NExp_CallInternalBinaryOperator>(info.rOperator, std::move(castExp0), std::move(castExp1));
            return MakePtr<NExp_CallInternalUnaryOperator>(RInternalUnaryOperator::LogicalNot_Bool_Bool, std::move(equalExp));
        }
    }

    // 3. InternalOperator에서 검색            
    auto matchedInfos = context.GetBinOpInfos(exp.kind);
    for(auto& info : matchedInfos)
    {
        auto castExp0 = TryCastRExp(NExpPtr(operand0), info.operandType0, context);
        if (!castExp0) continue;

        auto castExp1 = TryCastRExp(NExpPtr(operand1), info.operandType1, context);
        if (!castExp1) continue;

        // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다

        return MakePtr<NExp_CallInternalBinaryOperator>(info.rOperator, std::move(castExp0), std::move(castExp1));
    }

    // Operator를 찾을 수 없습니다
    context.Log(&Logger::Fatal_BinaryOp_OperatorNotFound);
    return nullptr;
}

NExpPtr TranslateSLambdaExpToNExp(SExp_Lambda& sExp, TranslationContext& context)
{
    // TODO: 리턴 타입과 인자타입은 타입 힌트를 반영해야 한다
    //RTypePtr retType = nullptr;
    
    //auto oLambdaInfo = TranslateLambda(retType, sExp.params, sExp.body, context);

    //if (!oLambdaInfo)
    //    return nullptr;

    // return MakePtr<RLambdaExp>(lambdaInfo.lambda, lambdaInfo.args), context.factory->MakeIn);
    static_assert(false);
}

NExpPtr TranslateSListExpToNExp(SExp_List& exp, TranslationContext& context)
{
    vector<NExpPtr> elems;
    elems.reserve(exp.elements.size());

    // TODO: 타입 힌트도 이용해야 할 것 같다
    RTypePtr curElemType = nullptr;

    for(auto& elem : exp.elements)
    {
        auto nElem = TranslateSExpToNExp(*elem, /*hintType*/ nullptr, context);
        if (!nElem) return nullptr;

        auto rElemType = context.GetType(*nElem);
        elems.push_back(std::move(nElem));

        if (curElemType == nullptr)
        {
            curElemType = std::move(rElemType);
            continue;
        }

        if (curElemType != rElemType)
        {
            context.Log(&Logger::Fatal_ListExp_MismatchBetweenElementTypes);
            return nullptr;
        }
    }

    if (curElemType == nullptr)
    {
        context.Log(&Logger::Fatal_ListExp_CantInferElementTypeWithEmptyElement);
        return nullptr;
    }

    return MakePtr<NExp_List>(std::move(elems), std::move(curElemType));
}

NExpPtr TranslateSNewExpToNExp(SExp_New& exp, TranslationContext& context) // throws ErrorCodeException
{
    auto rType = context.TranslateSTypeExpToRType(*exp.type);
    if (rType->GetCustomTypeKind() == RCustomTypeKind::Class)
    {
        context.Log(&Logger::Fatal_NewExp_TypeIsNotClass);
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

NExpPtr TranslateSCallExpToNExp(SExp_Call& exp, const RTypePtr& hintType, TranslationContext& context)
{
    auto imCallable = TranslateSExpToImExp(*exp.callable, hintType, context);
    if (!imCallable) return nullptr;

    return TranslateImCallableAndSArgsToNExp(*imCallable, exp.callable, exp.args, context); // 로깅할때 exp, exp.Callable두개가 다 필요할 수 있다
}

NExpPtr TranslateSBoxExpToNExp(SExp_Box& exp, const RTypePtr& hintType, TranslationContext& context)
{
    auto* hintBoxPtrType = dynamic_cast<RType_BoxPtr*>(hintType.get());
    auto innerHintType = hintBoxPtrType ? hintBoxPtrType->innerType : nullptr;

    // hintType전수
    auto nInnerExp = TranslateSExpToNExp(*exp.innerExp, innerHintType, context);
    if (!nInnerExp) return nullptr;

    return MakePtr<NExp_Box>(std::move(nInnerExp));
}

NExpPtr TranslateSIsExpToNExp(SExp_Is& exp, TranslationContext& context)
{
    auto target = TranslateSExpToNExp(*exp.exp, /*hintType*/ nullptr, context);
    if (!target) return nullptr;

    auto targetType = context.GetType(*target);
    auto targetTypeKind = targetType->GetCustomTypeKind();

    auto testType = context.TranslateSTypeExpToRType(*exp.type);
    auto testTypeKind = testType->GetCustomTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RCustomTypeKind::Class)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<NExp_ClassIsClass>(std::move(target), testType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<NExp_InterfaceIsClass>(std::move(target), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::Interface)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<NExp_ClassIsInterface>(std::move(target), testType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<NExp_InterfaceIsInterface>(std::move(target), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::EnumElem)
    {
        if (targetTypeKind == RCustomTypeKind::Enum)
            return MakePtr<NExp_EnumIsEnumElem>(std::move(target), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else
        throw NotImplementedException(); // 에러 처리
}

NExpPtr TranslateSAsExpToNExp(SExp_As& exp, TranslationContext& context)
{
    auto nTarget = TranslateSExpToNExp(*exp.exp, /* hintType */ nullptr, context);
    if (!nTarget) return nullptr;    

    auto rTestType = context.TranslateSTypeExpToRType(*exp.type);

    return context.MakeNExp_As(std::move(nTarget), rTestType);
}

namespace {

// S.Exp -> R.Exp
class SExpToNExpTranslator : public SExpVisitor
{
    RTypePtr hintType;
    NExpPtr* result;

    TranslationContext& context;

public:
    SExpToNExpTranslator(const RTypePtr& hintType, NExpPtr* result, TranslationContext& context)
        : hintType(hintType), result(result), context(context)
    {
    }

    // S.Exp -> IntermediateExp -> ResolvedExp -> R.Exp
    void HandleDefault(SExp& exp)
    {
        auto reExp = TranslateSExpToReExp(exp, hintType, context);

        if (reExp)
            *result = TranslateReExpToNExp(*reExp, context);
        else
            *result = nullptr;
    }

    void Visit(SExp_Identifier& exp) override
    {
        return HandleDefault(exp);
    }

    void Visit(SExp_String& exp) override
    {
        *result = TranslateSStringExpToNStringExp(exp, context);
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        *result = TranslateSIntLiteralExpToNExp(exp);
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        *result = TranslateSBoolLiteralExpToNExp(exp);
    }

    void Visit(SExp_NullLiteral& exp) override
    {
        *result = TranslateSNullLiteralExpToNExp(exp, hintType, context);
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        *result = TranslateSBinaryOpExpToNExp(exp, context);
    }

    void Visit(SExp_UnaryOp& exp) override
    {
        if (exp.kind == SUnaryOpKind::Deref)
            return HandleDefault(exp);

        *result = TranslateSUnaryOpExpToNExpExceptDeref(exp, context);
    }

    void Visit(SExp_Call& exp) override
    {
        *result = TranslateSCallExpToNExp(exp, hintType, context);
    }

    void Visit(SExp_Lambda& exp) override
    {
        // context.SetSyntax(syntax);
        *result = TranslateSLambdaExpToNExp(exp, context);
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
        *result = TranslateSListExpToNExp(exp, context);
    }

    void Visit(SExp_New& exp) override
    {
        *result = TranslateSNewExpToNExp(exp, context);
    }

    void Visit(SExp_Box& exp) override
    {
        *result = TranslateSBoxExpToNExp(exp, hintType, context);
    }

    void Visit(SExp_Is& exp) override
    {
        *result = TranslateSIsExpToNExp(exp, context);
    }

    void Visit(SExp_As& exp) override
    {
        *result = TranslateSAsExpToNExp(exp, context);
    }
};

} // namespace 

NExpPtr TranslateSExpToNExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context)
{
    NExpPtr nExp;
    SExpToNExpTranslator translator(hintType, &nExp, context);
    exp.Accept(translator);
    return nExp;
}

}