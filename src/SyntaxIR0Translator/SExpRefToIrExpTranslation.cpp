module Citron.SyntaxIR0Translator:SExpRefToIrExpTranslation;

import <expected>;

import Citron.Ptr;
import Citron.Syntax;
import Citron.Logger;
import Citron.Diag;

import :IrExp;

import :SExpToNExpTranslation;
import :SExpToNLocTranslation;
import :SExpRefToNExpTranslation;
import :IrExpAndMemberNameToIrExpTranslation;

import :TranslationContext;
import :Misc;

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

// & exp syntax를 중간과정으로 번역해주는 역할
// SExp -> IrExp
struct SExpRefToIrExpTranslator : public SExpVisitor
{
    expected<IrExpPtr, DiagPtr>* result;
    TranslationContext& context;

public:
    SExpRefToIrExpTranslator(std::expected<IrExpPtr, DiagPtr>* result, TranslationContext& context)
        : result(result), context(context)
    {
    }

private:
    void Value(IrExpPtr&& nExp)
    {
        *result = std::move(nExp);
    }

    void Forward(expected<IrExpPtr, DiagPtr>&& r)
    {
        *result = std::move(r);
    }

    void Error(const DiagPtr& diag)
    {
        *result = unexpected{diag};
    }

    void HandleValue(SExp& exp)
    {
        auto nExp = TranslateSExpToNExp(exp, /*hintType*/ nullptr, context);
        if (!nExp)
        {
            *result = unexpected{nExp.error()};
            return;
        }

        *result = MakePtr<IrExp_LocalValue>(std::move(*nExp));
    }

public:
    // identifier에 &가 붙으면 어떻게 처리할 것인가
    void Visit(SExp_Identifier& exp) override
    {   
        // identifier는 name<typeArgs>로 이뤄져 있다
        static_assert(false);

        //try
        //{
        //    // syntax로 typeArgs를 만든다
        //    auto typeArgs = MakeTypeArgs(exp.typeArgs, context);           

        //    // ResolveIdentifier는 에러를 어떻게 리턴하는가
        //    // 1. 실행중에 에러가 발생하면, 에러를 로깅하고 바로 리턴한다
        //    // 2. 바로 리턴하면서 에러를 같이 리턴한다

        //    // 1이면, try를 할때마다 logger인스턴스를 새로 생성해야 한다
        //    // 
        //    // 2이면, 에러가 늦게 출력된다. nested error처리를 하는것이 좋겠다. 프로그램 작성이 복잡해진다
        //    //   에러를 던져야 좀 깔끔하게 될지도 모르겠다
        //    
        //    // 2번이 나은것 같다
        //    if (auto result = context.ResolveIdentifier(RName_Normal{exp.value}, std::move(typeArgs)); result)
        //    {
        //        auto& imExp = result.value();

        //    }
        //    else
        //    {
        //        logger->Fatal_ResoveIdentifier();

        //        context.MakeDesignatedErrorLogger

        //        return Fatal(A2007_ResolveIdentifier_NotFound, exp);
        //    }

        //    var imRefExp = TranslateImExpToIrExp(imExp, factory);
        //    if (imRefExp == null)
        //    {
        //        return Fatal(A3001_Reference_CantMakeReference, exp);
        //    }

        //    return Valid(imRefExp);
        //}
        //catch (IdentifierResolverMultipleCandidatesException)
        //{
        //    return Fatal(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, exp);
        //}
    }

    // string은 중간과정에서는 value로 평가하면 될 것 같다
    void Visit(SExp_String& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_NullLiteral& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        // assign 제외
        return HandleValue(exp);
    }

    void Visit(SExp_UnaryOp& exp) override
    {
        if (exp.kind == SUnaryOpKind::Ref) // & &는 불가능
        {
            auto nExp = TranslateSExpRefToNExp(*exp.operand, context);
            if (!nExp) return Error(nExp.error());

            return Value(MakePtr<IrExp_LocalValue>(std::move(nExp)));
        }
        else if (exp.kind == SUnaryOpKind::Deref) // *pS
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

            auto nOperandLoc = TranslateSExpToNLoc(exp, /*hintType*/ nullptr, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
            if (!nOperandLoc) return Error(nOperandLoc.error());

            return Value(MakePtr<IrExp_DerefedBoxValue>(std::move(nOperandLoc)));
        }
        else
        {
            return HandleValue(exp);
        }
    }

    void Visit(SExp_Call& exp) override
    {
        HandleValue(exp);
    }

    void Visit(SExp_Lambda& exp) override
    {
        HandleValue(exp);
    }

    // e[e] 꼴
    void Visit(SExp_Indexer& exp) override
    {
        // location으로 쓰지 않고 value로 쓴다
        HandleValue(exp);
    }

    void Visit(SExp_Member& exp) override
    {
        auto parent = TranslateSExpRefToIrExp(*exp.parent, context);
        if (!parent) return Error(parent.error());

        auto typeArgsExceptOuter = MakeTypeArgs(exp.memberTypeArgs, context);

        context.SetSyntax(exp.parent);
        return Forward(TranslateIrExpAndMemberNameToIrExp(*parent, RName_Normal(exp.memberName), std::move(typeArgsExceptOuter), context));
    }

    void Visit(SExp_IndirectMember& exp) override
    {
        static_assert(false);
    }

    void Visit(SExp_List& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_New& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_Box& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_Is& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_As& exp) override
    {
        return HandleValue(exp);
    }
};

} // namespace 

expected<IrExpPtr, DiagPtr> TranslateSExpRefToIrExp(SExp& exp, TranslationContext& context)
{
    expected<IrExpPtr, DiagPtr> irExp;
    SExpRefToIrExpTranslator translator(&irExp, context);
    exp.Accept(translator);

    return irExp;
}

} // namespace Citron::SyntaxIR0Translator