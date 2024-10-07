#include "pch.h"
#include "SExpToReExpTranslation.h"

#include <Infra/Ptr.h>
#include <Syntax/Syntax.h>
#include "SExpToImExpTranslation.h"
#include "ImExpToReExpTranslation.h"
#include "ReExp.h"
#include "SExpToRExpTranslation.h"

namespace Citron::SyntaxIR0Translator {

class SExpToReExpTranslator : public SExpVisitor
{
    RTypePtr hintType;
    ReExpPtr* result;
    ScopeContextPtr context;
    LoggerPtr logger;
    RTypeFactory& factory;

public:
    SExpToReExpTranslator(const RTypePtr& hintType, ReExpPtr* result, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
        : hintType(hintType), result(result), context(context), logger(logger), factory(factory)
    {
    }

    void HandleDefault(SExp& exp)
    {
        auto imExp = TranslateSExpToImExp(exp, hintType, context);
        if (!imExp)
        {
            *result = nullptr;
            return;
        }

        *result = TranslateImExpToReExp(imExp, context, logger);
    }

    void HandleExp(RExpPtr&& exp)
    {
        if (!exp)
            *result = nullptr;
        else 
            *result = MakePtr<ReElseExp>(std::move(exp));
    }


    void Visit(SIdentifierExp& exp) override 
    {
        return HandleDefault(exp);
    }

    void Visit(SStringExp& exp) override 
    {
        HandleExp(TranslateSStringExpToRStringExp(exp, context, logger, factory));
    }

    void Visit(SIntLiteralExp& exp) override 
    {
        HandleExp(TranslateSIntLiteralExpToRExp(exp));
    }

    void Visit(SBoolLiteralExp& exp) override 
    {
        HandleExp(TranslateSBoolLiteralExpToRExp(exp));
    }

    void Visit(SNullLiteralExp& exp) override 
    {
        HandleExp(TranslateSNullLiteralExpToRExp(exp, hintType, context, logger));
    }

    void Visit(SBinaryOpExp& exp) override 
    {
        HandleExp(TranslateSBinaryOpExpToRExp(exp, context, logger, factory));
    }

    // int만 지원한다
    void Visit(SUnaryOpExp& exp) override 
    {
        if (exp.kind == SUnaryOpKind::Deref)
        {
            return HandleDefault(exp);
        }
        else
        {
            return HandleExp(TranslateSUnaryOpExpToRExpExceptDeref(exp, context, logger, factory));
        }
    }

    void Visit(SCallExp& exp) override 
    {
        return HandleExp(TranslateSCallExpToRExp(exp, hintType, context, logger));
    }

    void Visit(SLambdaExp& exp) override 
    {
        return HandleExp(TranslateSLambdaExpToRExp(exp, logger));
    }

    void Visit(SIndexerExp& exp) override 
    {
        return HandleDefault(exp);
    }

    // exp를 돌려주는 버전
    // parent."x"<>
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
        HandleExp(TranslateSListExpToRExp(exp, context, logger, factory));
    }

    // 'new C(...)'
    void Visit(SNewExp& exp) override 
    {
        HandleExp(TranslateSNewExpToRExp(exp, context, logger, factory));
    }

    void Visit(SBoxExp& exp) override 
    {
        HandleExp(TranslateSBoxExpToRExp(exp, hintType, context, logger, factory));
    }

    void Visit(SIsExp& exp) override 
    {
        HandleExp(TranslateSIsExpToRExp(exp, context, logger, factory));
    }

    void Visit(SAsExp& exp) override 
    {
        HandleExp(TranslateSAsExpToRExp(exp, context, logger, factory));
    }
};

ReExpPtr TranslateSExpToReExp(SExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory)
{
    ReExpPtr reExp;
    SExpToReExpTranslator translator(hintType, &reExp, context, logger, factory);
    exp.Accept(translator);
    return reExp;
}

} // namesapce Citron::SyntaxIR0Translator