#include "SExpToReExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "Logging/Diag.h"
#include "MIR/MExp.h"

#include "SExpToImExpTranslation.h"
#include "ImExpToReExpTranslation.h"
#include "ReExp.h"
#include "SExpToMExpTranslation.h"
#include "TranslationContexts.h"
#include "SRTFactory.h"

using namespace std;

namespace Citron {

namespace {

class SExpToReExpTranslator
{
public:
    using ResultType = expected<ReExp, DiagPtr>;

private:
    RType* hintType;
    TranslationContexts& contexts;

public:
    SExpToReExpTranslator(RType* hintType, TranslationContexts& contexts)
        : hintType{hintType}, contexts{contexts}
    {
    }

private:
    ResultType HandleDefault(SExp* exp)
    {
        auto e_imExp = TranslateSExpToImExp(exp, hintType, contexts);
        RETURN_ON_ERROR(e_imExp);

        return TranslateImExpToReExp(*e_imExp, contexts);
    }

    ResultType HandleExp(expected<MExp*, DiagPtr>&& eExp)
    {
        if (!eExp)
            return unexpected{move(eExp).error()};
        else
            return ReExp_Exp{*eExp};
    }

public:
    ResultType Visit(SExp_Identifier* exp)
    {
        return HandleDefault(exp);
    }

    ResultType Visit(SExp_String* exp)
    {
        return HandleExp(TranslateSStringExpToMStringExp(exp, contexts));
    }

    ResultType Visit(SExp_IntLiteral* exp)
    {
        return HandleExp(TranslateSIntLiteralExpToMExp(exp, contexts));
    }

    ResultType Visit(SExp_BoolLiteral* exp)
    {
        return HandleExp(TranslateSBoolLiteralExpToMExp(exp, contexts));
    }

    ResultType Visit(SExp_NullLiteral* exp)
    {
        return HandleExp(TranslateSNullLiteralExpToMExp(exp, hintType, contexts));
    }

    ResultType Visit(SExp_BinaryOp* exp)
    {
        return HandleExp(TranslateSBinaryOpExpToMExp(exp, contexts));
    }

    // int만 지원한다
    ResultType Visit(SExp_UnaryOp* exp)
    {
        if (exp->kind == SUnaryOpKind::Deref)
        {
            return HandleDefault(exp);
        }
        else
        {
            return HandleExp(TranslateSUnaryOpExpToMExpExceptDeref(exp, contexts));
        }
    }

    ResultType Visit(SExp_Call* exp)
    {
        return HandleExp(TranslateSCallExpToMExp(exp, hintType, contexts));
    }

    ResultType Visit(SExp_Lambda* exp)
    {
        return HandleExp(TranslateSLambdaExpToMExp(exp, contexts));
    }

    ResultType Visit(SExp_Indexer* exp)
    {
        return HandleDefault(exp);
    }

    // exp를 돌려주는 버전
    // parent."x"<>
    ResultType Visit(SExp_Member* exp)
    {
        return HandleDefault(exp);
    }

    ResultType Visit(SExp_IndirectMember* exp)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(SExp_List* exp)
    {
        return HandleExp(TranslateSListExpToMExp(exp, contexts));
    }

    // 'new C(...)'
    ResultType Visit(SExp_New* exp)
    {
        return HandleExp(TranslateSNewExpToMExp(exp, contexts));
    }

    ResultType Visit(SExp_Box* exp)
    {
        return HandleExp(TranslateSBoxExpToMExp(exp, hintType, contexts));
    }

    ResultType Visit(SExp_Is* exp)
    {
        return HandleExp(TranslateSIsExpToMExp(exp, contexts));
    }

    ResultType Visit(SExp_As* exp)
    {
        return HandleExp(TranslateSAsExpToMExp(exp, contexts));
    }
};

} // namespace

expected<ReExp, DiagPtr> TranslateSExpToReExp(SExp* exp, RType* hintType, TranslationContexts& contexts)
{
    SExpToReExpTranslator translator{hintType, contexts};
    return Accept(translator, exp);
}

} // namesapce Citron::SyntaxIR0Translator