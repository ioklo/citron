#include "pch.h"
#include "SExpToRLocTranslation.h"

#include <Infra/Ptr.h>
#include <Logging/Logger.h>
#include <Syntax/Syntax.h>
#include <IR0/RLoc.h>

#include "DesignatedErrorLogger.h"
#include "ReExpToRLocTranslation.h"
#include "SExpToReExpTranslation.h"
#include "SExpToRExpTranslation.h"

#include "TranslationContext.h"

namespace Citron::SyntaxIR0Translator {

namespace {

class SExpToRLocTranslator : public SExpVisitor
{
    RTypePtr hintType;
    bool bWrapExpAsLoc;

    IDesignatedErrorLogger* notLocationErrorLogger;
    RLocPtr* result;

    TranslationContext& context;

public:
    SExpToRLocTranslator(
        const RTypePtr& hintType,
        bool bWrapExpAsLoc,
        IDesignatedErrorLogger* notLocationErrorLogger,
        RLocPtr* result,
        TranslationContext& context)
        : hintType(hintType), bWrapExpAsLoc(bWrapExpAsLoc), notLocationErrorLogger(notLocationErrorLogger), result(result), context(context)
    {
    }

    void HandleDefault(SExp& sExp)
    {
        if (auto reExp = TranslateSExpToReExp(sExp, hintType, context))
        {
            DesignatedErrorLogger designatedErrorLogger(*context.logger, &Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);
            *result = TranslateReExpToRLoc(*reExp, bWrapExpAsLoc, &designatedErrorLogger, context);
        }
        else // invalid
        {
            *result = nullptr;
        }
    }

    // fast track
    void HandleExp(RExpPtr&& rExp)
    {
        if (!rExp)
        {
            *result = nullptr;
        }
        else if (bWrapExpAsLoc)
        {
            *result = MakePtr<RLoc_Temp>(std::move(rExp));
        }
        else
        {
            notLocationErrorLogger->Log();
            *result = nullptr;
        }
    }

    void Visit(SExp_Identifier& exp) override
    {
        HandleDefault(exp);
    }

    void Visit(SExp_String& exp) override
    {
        auto rExp = TranslateSStringExpToRStringExp(exp, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        auto rExp = TranslateSIntLiteralExpToRExp(exp);
        HandleExp(std::move(rExp));
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        auto rExp = TranslateSBoolLiteralExpToRExp(exp);
        HandleExp(std::move(rExp));
    }

    void Visit(SExp_NullLiteral& exp) override
    {
        auto rExp = TranslateSNullLiteralExpToRExp(exp, hintType, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        RExpPtr rExp = TranslateSBinaryOpExpToRExp(exp, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SExp_UnaryOp& exp) override
    {
        // Deref는 loc으로 변경되어야 한다
        if (exp.kind == SUnaryOpKind::Deref)
        {
            HandleDefault(exp);
        }
        else
        {
            auto rExp = TranslateSUnaryOpExpToRExpExceptDeref(exp, context);
            HandleExp(std::move(rExp));
        }
    }

    void Visit(SExp_Call& exp) override
    {
        RExpPtr rExp = TranslateSCallExpToRExp(exp, hintType, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SExp_Lambda& exp) override
    {
        auto rExp = TranslateSLambdaExpToRExp(exp, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SExp_Indexer& exp) override
    {
        HandleDefault(exp);
    }

    void Visit(SExp_Member& exp) override
    {
        HandleDefault(exp);
    }

    // s->x
    void Visit(SExp_IndirectMember& exp) override { static_assert(false); }

    void Visit(SExp_List& exp) override
    {
        auto rExp = TranslateSListExpToRExp(exp, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SExp_New& exp) override
    {
        auto rExp = TranslateSNewExpToRExp(exp, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SExp_Box& exp) override
    {
        auto rExp = TranslateSBoxExpToRExp(exp, hintType, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SExp_Is& exp) override
    {
        auto rExp = TranslateSIsExpToRExp(exp, context);
        HandleExp(std::move(rExp));
    }

    void Visit(SExp_As& exp) override
    {
        auto rExp = TranslateSAsExpToRExp(exp, context);
        HandleExp(std::move(rExp));
    }
};

} // namespace 

RLocPtr TranslateSExpToRLoc(SExp& sExp, const RTypePtr& hintType, bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationLogger, TranslationContext& context)
{
    RLocPtr rLoc;
    SExpToRLocTranslator translator(hintType, bWrapExpAsLoc, notLocationLogger, &rLoc, context);
    sExp.Accept(translator);

    return rLoc;
}


} // namespace Citron::SyntaxIR0Translator
