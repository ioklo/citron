module Citron.SyntaxIR0Translator:SExpToNLocTranslation;

import <expected>;

import Citron.Ptr;
import Citron.Exceptions;
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
        if (auto eReExp = TranslateSExpToReExp(sExp, hintType, context))
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            *result = TranslateReExpToNLoc(**eReExp, bWrapExpAsLoc, &designatedDiag, context);
        }
        else // invalid
        {
            *result = unexpected{move(eReExp).error()};
        }
    }

    // fast track
    void HandleExp(expected<NExpPtr, DiagPtr>&& eNExp)
    {
        if (!eNExp)
        {
            *result = unexpected{move(eNExp).error()};
        }
        else if (bWrapExpAsLoc)
        {
            *result = MakePtr<NLoc_Temp>(move(*eNExp));
        }
        else
        {
            *result = unexpected{notLocationDiag->MakeDiag()};
        }
    }

    template<typename TValue>
    void Error(expected<TValue, DiagPtr>&& e)
    {
        *result = unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::is_base_of_v<Diag, TDiag>
    void Error(TArgs&&... args)
    {
        *result = unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    void Visit(SExp_Identifier& exp) override
    {
        return HandleDefault(exp);
    }

    void Visit(SExp_String& exp) override
    {
        auto eNExp = TranslateSStringExpToNStringExp(exp, context);
        if (!eNExp) return Error(move(eNExp));

        return HandleExp(move(*eNExp));
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        auto eNExp = TranslateSIntLiteralExpToNExp(exp);
        if (!eNExp) return Error(move(eNExp));

        return HandleExp(move(*eNExp));
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        auto eNExp = TranslateSBoolLiteralExpToNExp(exp);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    void Visit(SExp_NullLiteral& exp) override
    {
        auto eNExp = TranslateSNullLiteralExpToNExp(exp, hintType, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        auto eNExp = TranslateSBinaryOpExpToNExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
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
            auto eNExp = TranslateSUnaryOpExpToNExpExceptDeref(exp, context);
            if (!eNExp) return Error(move(eNExp));
            return HandleExp(move(*eNExp));
        }
    }

    void Visit(SExp_Call& exp) override
    {
        auto eNExp = TranslateSCallExpToNExp(exp, hintType, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    void Visit(SExp_Lambda& exp) override
    {
        auto eNExp = TranslateSLambdaExpToNExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
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
        throw NotImplementedException();
    }

    void Visit(SExp_List& exp) override
    {
        auto eNExp = TranslateSListExpToNExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    void Visit(SExp_New& exp) override
    {
        auto eNExp = TranslateSNewExpToNExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    void Visit(SExp_Box& exp) override
    {
        auto eNExp = TranslateSBoxExpToNExp(exp, hintType, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    void Visit(SExp_Is& exp) override
    {
        auto eNExp = TranslateSIsExpToNExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    void Visit(SExp_As& exp) override
    {
        auto eNExp = TranslateSAsExpToNExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
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
