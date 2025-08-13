#include "SExpToReExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Syntax/Syntax.h"
#include "Logging/Diag.h"
#include "IR0/NExp.h"

#include "SExpToImExpTranslation.h"
#include "ImExpToReExpTranslation.h"
#include "ReExp.h"
#include "SExpToNExpTranslation.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

class SExpToReExpTranslator : public SExpVisitor
{
    expected<ReExp*, DiagPtr>* result;

    RType* hintType;
    TranslationContext& context;

public:
    SExpToReExpTranslator(expected<ReExp*, DiagPtr>* result, RType* hintType, TranslationContext& context)
        : result(result), hintType(hintType), context(context)
    {
    }

private:
    void HandleDefault(SExp& exp)
    {
        auto eImExp = TranslateSExpToImExp(exp, hintType, context);
        if (!eImExp)
        {
            *result = unexpected{move(eImExp).error()};
            return;
        }

        *result = TranslateImExpToReExp(**eImExp, context);
    }

    void HandleExp(expected<NExp*, DiagPtr>&& eExp)
    {
        if (!eExp)
            *result = unexpected{move(eExp).error()};
        else
            *result = MakePtr<ReExp_Else>(move(*eExp));
    }

public:
    void Visit(SExp_Identifier& exp) override
    {
        return HandleDefault(exp);
    }

    void Visit(SExp_String& exp) override
    {
        return HandleExp(TranslateSStringExpToNStringExp(exp, context));
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        return HandleExp(TranslateSIntLiteralExpToNExp(exp));
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        return HandleExp(TranslateSBoolLiteralExpToNExp(exp));
    }

    void Visit(SExp_NullLiteral& exp) override
    {
        return HandleExp(TranslateSNullLiteralExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        return HandleExp(TranslateSBinaryOpExpToNExp(exp, context));
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
        throw NotImplementedException();
    }

    void Visit(SExp_List& exp) override
    {
        return HandleExp(TranslateSListExpToNExp(exp, context));
    }

    // 'new C(...)'
    void Visit(SExp_New& exp) override
    {
        return HandleExp(TranslateSNewExpToNExp(exp, context));
    }

    void Visit(SExp_Box& exp) override
    {
        return HandleExp(TranslateSBoxExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_Is& exp) override
    {
        return HandleExp(TranslateSIsExpToNExp(exp, context));
    }

    void Visit(SExp_As& exp) override
    {
        return HandleExp(TranslateSAsExpToNExp(exp, context));
    }
};

} // namespace

expected<ReExp*, DiagPtr> TranslateSExpToReExp(SExp& exp, RType* hintType, TranslationContext& context)
{
    expected<ReExp*, DiagPtr> reExp;
    SExpToReExpTranslator translator{&reExp, hintType, context};
    exp.Accept(translator);
    return reExp;
}

} // namesapce Citron::SyntaxIR0Translator