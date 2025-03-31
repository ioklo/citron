#include "pch.h"
#include "SExpToReExpTranslation.h"

import Citron.Ptr;
#include <Syntax/Syntax.h>
#include "SExpToImExpTranslation.h"
#include "ImExpToReExpTranslation.h"
#include "ReExp.h"
#include "SExpToNExpTranslation.h"

namespace Citron::SyntaxIR0Translator {

namespace {

class SExpToReExpTranslator : public SExpVisitor
{
    RTypePtr hintType;
    ReExpPtr* result;
    TranslationContext& context;

public:
    SExpToReExpTranslator(const RTypePtr& hintType, ReExpPtr* result, TranslationContext& context)
        : hintType(hintType), result(result), context(context)
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

        *result = TranslateImExpToReExp(*imExp, context);
    }

    void HandleExp(NExpPtr&& exp)
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
        HandleExp(TranslateSStringExpToNStringExp(exp, context));
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        HandleExp(TranslateSIntLiteralExpToNExp(exp));
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        HandleExp(TranslateSBoolLiteralExpToNExp(exp));
    }

    void Visit(SExp_NullLiteral& exp) override
    {
        HandleExp(TranslateSNullLiteralExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        HandleExp(TranslateSBinaryOpExpToNExp(exp, context));
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
            return HandleExp(TranslateSUnaryOpExpToNExpExceptDeref(exp, context));
        }
    }

    void Visit(SExp_Call& exp) override
    {
        return HandleExp(TranslateSCallExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_Lambda& exp) override
    {
        return HandleExp(TranslateSLambdaExpToNExp(exp, context));
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
        HandleExp(TranslateSListExpToNExp(exp, context));
    }

    // 'new C(...)'
    void Visit(SExp_New& exp) override
    {
        HandleExp(TranslateSNewExpToNExp(exp, context));
    }

    void Visit(SExp_Box& exp) override
    {
        HandleExp(TranslateSBoxExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_Is& exp) override
    {
        HandleExp(TranslateSIsExpToNExp(exp, context));
    }

    void Visit(SExp_As& exp) override
    {
        HandleExp(TranslateSAsExpToNExp(exp, context));
    }
};

} // namespace

ReExpPtr TranslateSExpToReExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context)
{
    ReExpPtr reExp;
    SExpToReExpTranslator translator(hintType, &reExp, context);
    exp.Accept(translator);
    return reExp;
}

} // namesapce Citron::SyntaxIR0Translator