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

        *result = TranslateImExpToReExp(*imExp, context, logger);
    }

    void HandleExp(RExpPtr&& exp)
    {
        if (!exp)
            *result = nullptr;
        else 
            *result = MakePtr<ReExp_Else>(std::move(exp));
    }


    void Visit(SExp_Identifier& exp) override 
    {
        return HandleDefault(exp);
    }

    void Visit(SExp_String& exp) override 
    {
        HandleExp(TranslateSStringExpToRStringExp(exp, context, logger, factory));
    }

    void Visit(SExp_IntLiteral& exp) override 
    {
        HandleExp(TranslateSIntLiteralExpToRExp(exp));
    }

    void Visit(SExp_BoolLiteral& exp) override 
    {
        HandleExp(TranslateSBoolLiteralExpToRExp(exp));
    }

    void Visit(SExp_NullLiteral& exp) override 
    {
        HandleExp(TranslateSNullLiteralExpToRExp(exp, hintType, context, logger));
    }

    void Visit(SExp_BinaryOp& exp) override 
    {
        HandleExp(TranslateSBinaryOpExpToRExp(exp, context, logger, factory));
    }

    // int만 지원한다
    void Visit(SExp_UnaryOp& exp) override 
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

    void Visit(SExp_Call& exp) override 
    {
        return HandleExp(TranslateSCallExpToRExp(exp, hintType, context, logger, factory));
    }

    void Visit(SExp_Lambda& exp) override 
    {
        return HandleExp(TranslateSLambdaExpToRExp(exp, logger));
    }

    void Visit(SExp_Indexer& exp) override 
    {
        return HandleDefault(exp);
    }

    // exp를 돌려주는 버전
    // parent."x"<>
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
        HandleExp(TranslateSListExpToRExp(exp, context, logger, factory));
    }

    // 'new C(...)'
    void Visit(SExp_New& exp) override 
    {
        HandleExp(TranslateSNewExpToRExp(exp, context, logger, factory));
    }

    void Visit(SExp_Box& exp) override 
    {
        HandleExp(TranslateSBoxExpToRExp(exp, hintType, context, logger, factory));
    }

    void Visit(SExp_Is& exp) override 
    {
        HandleExp(TranslateSIsExpToRExp(exp, context, logger, factory));
    }

    void Visit(SExp_As& exp) override 
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