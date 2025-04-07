module Citron.SyntaxIR0Translator:SExpToNLocTranslation;

import <expected>;

import Citron.Ptr;
import Citron.Logger;
import Citron.Syntax;
import Citron.NDecls;

import :DesignatedDiagnostic;
import :ReExpToNLocTranslation;
import :SExpToReExpTranslation;
import :SExpToNExpTranslation;

import :TranslationContext;

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

class SExpToNLocTranslator : public SExpVisitor
{
    expected<NLocPtr, DiagPtr>* result;
    RTypePtr hintType;
    bool bWrapExpAsLoc;

    IDesignatedDiagnostic* notLocationDiag;
    TranslationContext& context;

public:
    SExpToNLocTranslator(
        expected<NLocPtr, DiagPtr>* result,
        const RTypePtr& hintType,
        bool bWrapExpAsLoc,
        IDesignatedDiagnostic* notLocationDiag,
        TranslationContext& context)
        : result(result), hintType(hintType), bWrapExpAsLoc(bWrapExpAsLoc), notLocationDiag(notLocationDiag), context(context)
    {
    }

private:
    void HandleDefault(SExp& sExp)
    {
        if (auto reExp = TranslateSExpToReExp(sExp, hintType, context))
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            *result = TranslateReExpToNLoc(**reExp, bWrapExpAsLoc, &designatedDiag, context);
        }
        else // invalid
        {
            *result = unexpected{reExp.error()};
        }
    }

    // fast track
    void HandleExp(expected<NExpPtr, DiagPtr>&& nExp)
    {
        if (!nExp)
        {
            *result = unexpected{nExp.error()};
        }
        else if (bWrapExpAsLoc)
        {
            *result = MakePtr<NLoc_Temp>(std::move(*nExp));
        }
        else
        {
            *result = unexpected{notLocationDiag->MakeDiag()};
        }
    }

    void Error(const DiagPtr& diag)
    {
        *result = unexpected{diag};
    }

public:
    void Visit(SExp_Identifier& exp) override
    {
        return HandleDefault(exp);
    }

    void Visit(SExp_String& exp) override
    {
        auto nExp = TranslateSStringExpToNStringExp(exp, context);
        if (!nExp) return Error(nExp.error());

        return HandleExp(std::move(*nExp));
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        auto nExp = TranslateSIntLiteralExpToNExp(exp);
        if (!nExp) return Error(nExp.error());

        return HandleExp(std::move(*nExp));
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        auto nExp = TranslateSBoolLiteralExpToNExp(exp);
        if (!nExp) return Error(nExp.error());
        return HandleExp(std::move(*nExp));
    }

    void Visit(SExp_NullLiteral& exp) override
    {
        auto nExp = TranslateSNullLiteralExpToNExp(exp, hintType, context);
        if (!nExp) return Error(nExp.error());
        return HandleExp(std::move(*nExp));
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        auto nExp = TranslateSBinaryOpExpToNExp(exp, context);
        if (!nExp) return Error(nExp.error());
        return HandleExp(std::move(*nExp));
    }

    void Visit(SExp_UnaryOp& exp) override
    {
        // Deref는 loc으로 변경되어야 한다
        if (exp.kind == SUnaryOpKind::Deref)
        {
            return HandleDefault(exp);
        }
        else
        {
            auto nExp = TranslateSUnaryOpExpToNExpExceptDeref(exp, context);
            if (!nExp) return Error(nExp.error());
            return HandleExp(std::move(*nExp));
        }
    }

    void Visit(SExp_Call& exp) override
    {
        auto nExp = TranslateSCallExpToNExp(exp, hintType, context);
        if (!nExp) return Error(nExp.error());
        return HandleExp(std::move(*nExp));
    }

    void Visit(SExp_Lambda& exp) override
    {
        auto nExp = TranslateSLambdaExpToNExp(exp, context);
        if (!nExp) return Error(nExp.error());
        return HandleExp(std::move(*nExp));
    }

    void Visit(SExp_Indexer& exp) override
    {
        return HandleDefault(exp);
    }

    void Visit(SExp_Member& exp) override
    {
        return HandleDefault(exp);
    }

    // s->x
    void Visit(SExp_IndirectMember& exp) override 
    { 
        static_assert(false); 
    }

    void Visit(SExp_List& exp) override
    {
        auto nExp = TranslateSListExpToNExp(exp, context);
        if (!nExp) return Error(nExp.error());
        return HandleExp(std::move(*nExp));
    }

    void Visit(SExp_New& exp) override
    {
        auto nExp = TranslateSNewExpToNExp(exp, context);
        if (!nExp) return Error(nExp.error());
        return HandleExp(std::move(*nExp));
    }

    void Visit(SExp_Box& exp) override
    {
        auto nExp = TranslateSBoxExpToNExp(exp, hintType, context);
        if (!nExp) return Error(nExp.error());
        return HandleExp(std::move(*nExp));
    }

    void Visit(SExp_Is& exp) override
    {
        auto nExp = TranslateSIsExpToNExp(exp, context);
        if (!nExp) return Error(nExp.error());
        return HandleExp(std::move(*nExp));
    }

    void Visit(SExp_As& exp) override
    {
        auto nExp = TranslateSAsExpToNExp(exp, context);
        if (!nExp) return Error(nExp.error());
        return HandleExp(std::move(*nExp));
    }
};

} // namespace 

expected<NLocPtr, DiagPtr> TranslateSExpToNLoc(SExp& sExp, const RTypePtr& hintType, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context)
{
    expected<NLocPtr, DiagPtr> nLoc;
    SExpToNLocTranslator translator{&nLoc, hintType, bWrapExpAsLoc, notLocationDiag, context};
    sExp.Accept(translator);
    return nLoc;
}


} // namespace Citron::SyntaxIR0Translator
