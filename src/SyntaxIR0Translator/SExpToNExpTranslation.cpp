module Citron.SyntaxIR0Translator:SExpToNExpTranslation;

import <variant>;
import <cassert>;

import Citron.Ptr;
import Citron.Exceptions;
import Citron.Unreachable;
import Citron.Logger;
import Citron.Syntax;

import Citron.RDecls;
import Citron.NDecls;

import :ReExp;
import :ImExp;

import :SExpToNLocTranslation;
import :SExpToReExpTranslation;
import :SExpToImExpTranslation;
import :SExpRefToNExpTranslation;

import :ReExpToNExpTranslation;
import :ReExpToNLocTranslation;

import :ImCallableAndSArgsToNExpTranslation;

import :ScopeContext;
import :Misc;
import :BinOpQueryService;
import :TranslationContext;

using namespace std;

namespace Citron::SyntaxIR0Translator {

// Syntax Exp -> IR0 Exp로 바꿔주는 기본적인 코드
// Deref를 적용하지 않는다. 따로 해주어야 한다

expected<NExpPtr, DiagPtr> TranslateSNullLiteralExpToNExp(SExp_NullLiteral& exp, const RTypePtr& hintType, TranslationContext& context)
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
    return unexpected{MakePtr<Error_Reference_CantMakeReference>()};
}

expected<NExpPtr, DiagPtr>TranslateSBoolLiteralExpToNExp(SExp_BoolLiteral& exp)
{
    return MakePtr<NExp_BoolLiteral>(exp.value);
}

expected<NExpPtr, DiagPtr>TranslateSIntLiteralExpToNExp(SExp_IntLiteral& exp)
{
    return MakePtr<NExp_IntLiteral>(exp.value);
}

expected<RStringExpElement, DiagPtr> TranslateSStringExpElementToRStringExpElement(const SStringExpElementPtr& elem, TranslationContext& context)
{
    // TranslationResult<R.StringExpElement> Valid(R.StringExpElement elem) = > TranslationResult.Valid(elem);
    // TranslationResult<R.StringExpElement> Error() = > TranslationResult.Error<R.StringExpElement>();
    // var stringType = context.GetStringType();

    context.SetSyntax(elem);

    if (auto* expElem = dynamic_cast<SStringExpElement_Exp*>(elem.get()))
    {
        auto reExp = TranslateSExpToReExp(*expElem->exp, /* hintType */ nullptr, context);
        if (!reExp) return unexpected{reExp.error()};

        auto reExpType = context.GetType(**reExp);

        // 캐스팅이 필요하다면 
        if (reExpType == context.MakeIntType())
        {   
            auto nExp = TranslateReExpToNExp(**reExp, context);
            if (!nExp) return unexpected{nExp.error()};

            return RLocStringExpElement(
                MakePtr<NLoc_Temp>(
                    MakePtr<NExp_CallInternalUnaryOperator>(RInternalUnaryOperator::ToString_Int_String, std::move(*nExp))));
        }
        else if (reExpType == context.MakeBoolType())
        {
            auto nExp = TranslateReExpToNExp(**reExp, context);
            if (!nExp) return unexpected{nExp.error()};

            return RLocStringExpElement(
                MakePtr<NLoc_Temp>(
                    MakePtr<NExp_CallInternalUnaryOperator>(RInternalUnaryOperator::ToString_Bool_String, std::move(*nExp))));
        }
        else if (reExpType == context.MakeStringType())
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

            auto nLoc = TranslateReExpToNLoc(**reExp, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
            if (!nLoc) return unexpected{nLoc.error()};

            return RLocStringExpElement(std::move(*nLoc));
        }
        else
        {
            // TODO: ToString
            return unexpected{MakePtr<Error_StringExp_ExpElementShouldBeBoolOrIntOrString>()};
        }
    }
    else if (auto* textElem = dynamic_cast<SStringExpElement_Text*>(elem.get()))
    {
        return RTextStringExpElement(textElem->text);
    }

    unreachable();
}

expected<shared_ptr<NExp_String>, DiagPtr> TranslateSStringExpToNStringExp(SExp_String& exp, TranslationContext& context)
{
    vector<DiagPtr> diags;
    vector<RStringExpElement> builder;
    for(auto& elem : exp.elements)
    {
        auto rStringExpElem = TranslateSStringExpElementToRStringExpElement(elem, context);

        if (!rStringExpElem)
        {
            diags.push_back(rStringExpElem.error());
            continue;
        }
        
        builder.push_back(std::move(*rStringExpElem));
    }

    if (!diags.empty())
        return unexpected{MakePtr<AggregateDiag>(std::move(diags))};

    return MakePtr<NExp_String>(std::move(builder));
}

// int만 지원한다
expected<NExpPtr, DiagPtr> TranslateSIntUnaryAssignExpToNExp(SExp& operand, RInternalUnaryAssignOperator op, TranslationContext& context)
{
    // exp를 loc으로 변환하는 일을 하면 안되지만, ref는 풀어야 한다
    // F()++; (x)
    // var& x = i; x++; (o)
    // throws NotLocationException
    
    DesignatedDiagnostic<Error_UnaryAssignOp_AssignableExpressionIsAllowedOnly> designatedDiag;
    auto nOperand = TranslateSExpToNLoc(operand, /* hintType */ nullptr, /* bWrapExpAsLoc */ false, &designatedDiag, context);
    if (!nOperand) return unexpected{nOperand.error()};

    // int type 검사, exact match
    if (context.GetType(**nOperand) != context.MakeIntType())
        return unexpected{MakePtr<Error_UnaryAssignOp_AssignableExpressionIsAllowedOnly>()};

    return MakePtr<NExp_CallInternalUnaryAssignOperator>(op, std::move(*nOperand));
}

expected<NExpPtr, DiagPtr> TranslateSUnaryOpExpToNExpExceptDeref(SExp_UnaryOp& sExp, TranslationContext& context)
{
    assert(sExp.kind != SUnaryOpKind::Deref);

    // ref 처리
    if (sExp.kind == SUnaryOpKind::Ref)
        return TranslateSExpRefToNExp(*sExp.operand, context);

    context.SetSyntax(sExp.operand);
    auto nOperand = TranslateSExpToNExp(*sExp.operand, /*hintType*/ nullptr, context);
    if (!nOperand) return unexpected{nOperand.error()};

    switch(sExp.kind)
    {
    case SUnaryOpKind::LogicalNot:
    {
        // exact match
        if (context.GetType(**nOperand) != context.MakeBoolType())
        {   
            return unexpected{MakePtr<Error_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly>()};
        }

        return MakePtr<NExp_CallInternalUnaryOperator>(RInternalUnaryOperator::LogicalNot_Bool_Bool, std::move(*nOperand));
    }

    case SUnaryOpKind::Minus:
    {
        if (context.GetType(**nOperand) != context.MakeIntType())
        {
            return unexpected{MakePtr<Error_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly>()};
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

expected<NExpPtr, DiagPtr> TranslateSAssignBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context)
{
    // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다
    context.SetSyntax(exp.operand0);
    DesignatedDiagnostic<Error_BinaryOp_LeftOperandIsNotAssignable> designatedDiag;
    auto nDestLoc = TranslateSExpToNLoc(*exp.operand0, /* hintType */ nullptr, /* bWrapExpAsLoc */ false, &designatedDiag, context);
    if (!nDestLoc) return unexpected{nDestLoc.error()};

    // 안되는거 체크
    auto* pNDestLoc = nDestLoc->get();
    if (dynamic_cast<NLoc_LambdaVar*>(pNDestLoc))
    {
        // int x = 0; var l = () { x = 3; }, TODO: 이거 가능하도록
        return unexpected{MakePtr<Error_BinaryOp_LeftOperandIsNotAssignable>()};
    }
    else if (dynamic_cast<NLoc_This*>(pNDestLoc))
    {
        return unexpected{MakePtr<Error_BinaryOp_LeftOperandIsNotAssignable>()};
    }
    else if (dynamic_cast<NLoc_Temp*>(pNDestLoc))
    {
        return unexpected{MakePtr<Error_BinaryOp_LeftOperandIsNotAssignable>()};
    }

    auto nDestLocType = context.GetType(**nDestLoc);
    context.SetSyntax(exp.operand1);
    auto nSrcExp = TranslateSExpToNExp(*exp.operand1, /*hintType*/ nDestLocType, context);
    if (!nSrcExp) return unexpected{nSrcExp.error()};

    auto nWrappedSrcExp = CastNExp(std::move(*nSrcExp), nDestLocType, context);
    if (!nWrappedSrcExp) return unexpected{nWrappedSrcExp.error()};

    return MakePtr<NExp_Assign>(std::move(*nDestLoc), std::move(*nWrappedSrcExp));
}

expected<NExpPtr, DiagPtr> TranslateSBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context)
{
    // 1. Assign 먼저 처리
    if (exp.kind == SBinaryOpKind::Assign)
    {
        return TranslateSAssignBinaryOpExpToNExp(exp, context);
    }

    auto operand0 = TranslateSExpToNExp(*exp.operand0, /*hintType*/ nullptr, context);
    if (!operand0) return unexpected{operand0.error()};

    auto operand1 = TranslateSExpToNExp(*exp.operand1, /*hintType*/ nullptr, context);
    if (!operand1) return unexpected{operand1.error()};

    // 2. NotEqual 처리
    if (exp.kind == SBinaryOpKind::NotEqual)
    {
        const auto& equalInfos = context.GetBinOpInfos(SBinaryOpKind::Equal);
        
        for(auto& info : equalInfos)
        {
            auto castExp0 = CastNExp(*operand0, info.operandType0, context);
            if (!castExp0) continue;

            auto castExp1 = CastNExp(*operand1, info.operandType1, context);
            if (!castExp1) continue;

            // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
            auto equalExp = MakePtr<NExp_CallInternalBinaryOperator>(info.rOperator, std::move(*castExp0), std::move(*castExp1));
            return MakePtr<NExp_CallInternalUnaryOperator>(RInternalUnaryOperator::LogicalNot_Bool_Bool, std::move(equalExp));
        }
    }

    // 3. InternalOperator에서 검색            
    auto matchedInfos = context.GetBinOpInfos(exp.kind);
    for(auto& info : matchedInfos)
    {
        auto castExp0 = CastNExp(*operand0, info.operandType0, context);
        if (!castExp0) continue;

        auto castExp1 = CastNExp(*operand1, info.operandType1, context);
        if (!castExp1) continue;

        // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다

        return MakePtr<NExp_CallInternalBinaryOperator>(info.rOperator, std::move(*castExp0), std::move(*castExp1));
    }

    // Operator를 찾을 수 없습니다
    return unexpected{MakePtr<Error_BinaryOp_OperatorNotFound>()};
}

expected<NExpPtr, DiagPtr> TranslateSLambdaExpToNExp(SExp_Lambda& sExp, TranslationContext& context)
{
    // TODO: 리턴 타입과 인자타입은 타입 힌트를 반영해야 한다
    //RTypePtr retType = nullptr;
    
    //auto oLambdaInfo = TranslateLambda(retType, sExp.params, sExp.body, context);

    //if (!oLambdaInfo)
    //    return nullptr;

    // return MakePtr<NLambdaExp>(lambdaInfo.lambda, lambdaInfo.args), context.factory->MakeIn);
    static_assert(false);
}

expected<NExpPtr, DiagPtr> TranslateSListExpToNExp(SExp_List& exp, TranslationContext& context)
{
    vector<NExpPtr> elems;
    elems.reserve(exp.elements.size());

    // TODO: 타입 힌트도 이용해야 할 것 같다
    RTypePtr curElemType = nullptr;

    for(auto& elem : exp.elements)
    {
        auto nElem = TranslateSExpToNExp(*elem, /*hintType*/ nullptr, context);
        if (!nElem) return unexpected{nElem.error()};

        auto rElemType = context.GetType(**nElem);
        elems.push_back(std::move(*nElem));

        if (curElemType == nullptr)
        {
            curElemType = std::move(rElemType);
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

    return MakePtr<NExp_List>(std::move(elems), std::move(curElemType));
}

expected<NExpPtr, DiagPtr> TranslateSNewExpToNExp(SExp_New& exp, TranslationContext& context) // throws ErrorCodeException
{
    auto rType = context.TranslateSTypeExpToRType(*exp.type);
    if (!rType) return unexpected{rType.error()};

    if ((*rType)->GetCustomTypeKind() == RCustomTypeKind::Class)
    {
        return unexpected{MakePtr<Error_NewExp_TypeIsNotClass>()};
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

expected<NExpPtr, DiagPtr> TranslateSCallExpToNExp(SExp_Call& exp, const RTypePtr& hintType, TranslationContext& context)
{
    auto imCallable = TranslateSExpToImExp(*exp.callable, hintType, context);
    if (!imCallable) return unexpected{imCallable.error()};

    return TranslateImCallableAndSArgsToNExp(**imCallable, exp.callable, exp.args, context); // 로깅할때 exp, exp.Callable두개가 다 필요할 수 있다
}

expected<NExpPtr, DiagPtr> TranslateSBoxExpToNExp(SExp_Box& exp, const RTypePtr& hintType, TranslationContext& context)
{
    auto* hintBoxPtrType = dynamic_cast<RType_BoxPtr*>(hintType.get());
    auto innerHintType = hintBoxPtrType ? hintBoxPtrType->innerType : nullptr;

    // hintType전수
    auto nInnerExp = TranslateSExpToNExp(*exp.innerExp, innerHintType, context);
    if (!nInnerExp) return unexpected{nInnerExp.error()};

    return MakePtr<NExp_Box>(std::move(*nInnerExp));
}

expected<NExpPtr, DiagPtr> TranslateSIsExpToNExp(SExp_Is& exp, TranslationContext& context)
{
    auto target = TranslateSExpToNExp(*exp.exp, /*hintType*/ nullptr, context);
    if (!target) return unexpected{target.error()};

    auto targetType = context.GetType(**target);
    auto targetTypeKind = targetType->GetCustomTypeKind();

    auto testType = context.TranslateSTypeExpToRType(*exp.type);
    if (!testType) return unexpected{testType.error()};

    auto testTypeKind = (*testType)->GetCustomTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RCustomTypeKind::Class)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<NExp_ClassIsClass>(std::move(*target), std::move(*testType));
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<NExp_InterfaceIsClass>(std::move(*target), std::move(*testType));
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::Interface)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<NExp_ClassIsInterface>(std::move(*target), std::move(*testType));
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<NExp_InterfaceIsInterface>(std::move(*target), std::move(*testType));
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::EnumElem)
    {
        if (targetTypeKind == RCustomTypeKind::Enum)
            return MakePtr<NExp_EnumIsEnumElem>(std::move(*target), std::move(*testType));
        else
            throw NotImplementedException(); // 에러 처리
    }
    else
        throw NotImplementedException(); // 에러 처리
}

expected<NExpPtr, DiagPtr> TranslateSAsExpToNExp(SExp_As& exp, TranslationContext& context)
{
    auto nTarget = TranslateSExpToNExp(*exp.exp, /* hintType */ nullptr, context);
    if (!nTarget) return unexpected{nTarget.error()};

    auto nTestType = context.TranslateSTypeExpToRType(*exp.type);
    if (!nTestType) return unexpected{nTestType.error()};

    return context.MakeNExp_As(std::move(*nTarget), *nTestType);
}

namespace {

// S.Exp -> R.Exp
class SExpToNExpTranslator : public SExpVisitor
{
    expected<NExpPtr, DiagPtr>* result;
    RTypePtr hintType;

    TranslationContext& context;

public:
    SExpToNExpTranslator(expected<NExpPtr, DiagPtr>* result, const RTypePtr& hintType, TranslationContext& context)
        : result(result), hintType(hintType), context(context)
    {
    }

private:
    // S.Exp -> IntermediateExp -> ResolvedExp -> R.Exp
    void HandleDefault(SExp& exp)
    {
        auto reExp = TranslateSExpToReExp(exp, hintType, context);

        if (reExp)
            *result = TranslateReExpToNExp(**reExp, context);
        else
            *result = nullptr;
    }

    void Forward(expected<NExpPtr, DiagPtr>&& r)
    {
        *result = std::move(r);
    }

public:
    void Visit(SExp_Identifier& exp) override
    {
        return HandleDefault(exp);
    }

    void Visit(SExp_String& exp) override
    {
        return Forward(TranslateSStringExpToNStringExp(exp, context));
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        return Forward(TranslateSIntLiteralExpToNExp(exp));
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        return Forward(TranslateSBoolLiteralExpToNExp(exp));
    }

    void Visit(SExp_NullLiteral& exp) override
    {
        return Forward(TranslateSNullLiteralExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        return Forward(TranslateSBinaryOpExpToNExp(exp, context));
    }

    void Visit(SExp_UnaryOp& exp) override
    {
        if (exp.kind == SUnaryOpKind::Deref)
            return HandleDefault(exp);

        return Forward(TranslateSUnaryOpExpToNExpExceptDeref(exp, context));
    }

    void Visit(SExp_Call& exp) override
    {
        return Forward(TranslateSCallExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_Lambda& exp) override
    {
        // context.SetSyntax(syntax);
        return Forward(TranslateSLambdaExpToNExp(exp, context));
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
        return Forward(TranslateSListExpToNExp(exp, context));
    }

    void Visit(SExp_New& exp) override
    {
        return Forward(TranslateSNewExpToNExp(exp, context));
    }

    void Visit(SExp_Box& exp) override
    {
        return Forward(TranslateSBoxExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_Is& exp) override
    {
        return Forward(TranslateSIsExpToNExp(exp, context));
    }

    void Visit(SExp_As& exp) override
    {
        return Forward(TranslateSAsExpToNExp(exp, context));
    }
};

} // namespace 

expected<NExpPtr, DiagPtr> TranslateSExpToNExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context)
{
    expected<NExpPtr, DiagPtr> result;

    SExpToNExpTranslator translator(&result, hintType, context);
    exp.Accept(translator);

    return result;
}

}