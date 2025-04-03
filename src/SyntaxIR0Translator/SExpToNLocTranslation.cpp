module Citron.SyntaxIR0Translator:SExpToNLocTranslation;

import Citron.Ptr;
import Citron.Logger;
import Citron.Syntax;
import Citron.NDecls;

import :DesignatedErrorLogger;
import :ReExpToNLocTranslation;
import :SExpToReExpTranslation;
import :SExpToNExpTranslation;

import :TranslationContext;

namespace Citron::SyntaxIR0Translator {

namespace {

class SExpToNLocTranslator : public SExpVisitor
{
    RTypePtr hintType;
    bool bWrapExpAsLoc;

    IDesignatedErrorLogger* notLocationErrorLogger;
    NLocPtr* result;

    TranslationContext& context;

public:
    SExpToNLocTranslator(
        const RTypePtr& hintType,
        bool bWrapExpAsLoc,
        IDesignatedErrorLogger* notLocationErrorLogger,
        NLocPtr* result,
        TranslationContext& context)
        : hintType(hintType), bWrapExpAsLoc(bWrapExpAsLoc), notLocationErrorLogger(notLocationErrorLogger), result(result), context(context)
    {
    }

    void HandleDefault(SExp& sExp)
    {
        if (auto reExp = TranslateSExpToReExp(sExp, hintType, context))
        {
            auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);
            *result = TranslateReExpToNLoc(*reExp, bWrapExpAsLoc, &designatedErrorLogger, context);
        }
        else // invalid
        {
            *result = nullptr;
        }
    }

    // fast track
    void HandleExp(NExpPtr&& rExp)
    {
        if (!rExp)
        {
            *result = nullptr;
        }
        else if (bWrapExpAsLoc)
        {
            *result = MakePtr<NLoc_Temp>(std::move(rExp));
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
        auto nExp = TranslateSStringExpToNStringExp(exp, context);
        HandleExp(std::move(nExp));
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        auto nExp = TranslateSIntLiteralExpToNExp(exp);
        HandleExp(std::move(nExp));
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        auto nExp = TranslateSBoolLiteralExpToNExp(exp);
        HandleExp(std::move(nExp));
    }

    void Visit(SExp_NullLiteral& exp) override
    {
        auto nExp = TranslateSNullLiteralExpToNExp(exp, hintType, context);
        HandleExp(std::move(nExp));
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        NExpPtr nExp = TranslateSBinaryOpExpToNExp(exp, context);
        HandleExp(std::move(nExp));
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
            auto nExp = TranslateSUnaryOpExpToNExpExceptDeref(exp, context);
            HandleExp(std::move(nExp));
        }
    }

    void Visit(SExp_Call& exp) override
    {
        NExpPtr nExp = TranslateSCallExpToNExp(exp, hintType, context);
        HandleExp(std::move(nExp));
    }

    void Visit(SExp_Lambda& exp) override
    {
        auto nExp = TranslateSLambdaExpToNExp(exp, context);
        HandleExp(std::move(nExp));
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
        auto nExp = TranslateSListExpToNExp(exp, context);
        HandleExp(std::move(nExp));
    }

    void Visit(SExp_New& exp) override
    {
        auto nExp = TranslateSNewExpToNExp(exp, context);
        HandleExp(std::move(nExp));
    }

    void Visit(SExp_Box& exp) override
    {
        auto nExp = TranslateSBoxExpToNExp(exp, hintType, context);
        HandleExp(std::move(nExp));
    }

    void Visit(SExp_Is& exp) override
    {
        auto nExp = TranslateSIsExpToNExp(exp, context);
        HandleExp(std::move(nExp));
    }

    void Visit(SExp_As& exp) override
    {
        auto nExp = TranslateSAsExpToNExp(exp, context);
        HandleExp(std::move(nExp));
    }
};

} // namespace 

NLocPtr TranslateSExpToNLoc(SExp& sExp, const RTypePtr& hintType, bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationLogger, TranslationContext& context)
{
    NLocPtr nLoc;
    SExpToNLocTranslator translator(hintType, bWrapExpAsLoc, notLocationLogger, &nLoc, context);
    sExp.Accept(translator);

    return nLoc;
}


} // namespace Citron::SyntaxIR0Translator
