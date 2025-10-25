#include "SExpToReExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Syntax/Syntax.h"
#include "Logging/Diag.h"
#include "MIR/MExp.h"

#include "SExpToImExpTranslation.h"
#include "ImExpToReExpTranslation.h"
#include "ReExp.h"
#include "SExpToMExpTranslation.h"
#include "TranslationContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

class SExpToReExpTranslator
{
public:
    using ResultType = expected<ReExp*, DiagPtr>;

private:
    RType* hintType;
    TranslationContext& context;

public:
    SExpToReExpTranslator(RType* hintType, TranslationContext& context)
        : hintType{hintType}, context{context}
    {
    }

private:
    ResultType HandleDefault(SExp* exp)
    {
        auto eImExp = TranslateSExpToImExp(exp, hintType, context);
        if (!eImExp)
        {
            return unexpected{move(eImExp).error()};
        }

        return TranslateImExpToReExp(*eImExp, context);
    }

    ResultType HandleExp(expected<MExp*, DiagPtr>&& eExp)
    {
        if (!eExp)
            return unexpected{move(eExp).error()};
        else
            return context.MakeReExp<ReExp_Else>(*eExp);
    }

public:
    ResultType Visit(SExp_Identifier* exp)
    {
        return HandleDefault(exp);
    }

    ResultType Visit(SExp_String* exp)
    {
        return HandleExp(TranslateSStringExpToNStringExp(exp, context));
    }

    ResultType Visit(SExp_IntLiteral* exp)
    {
        return HandleExp(TranslateSIntLiteralExpToMExp(exp, context));
    }

    ResultType Visit(SExp_BoolLiteral* exp)
    {
        return HandleExp(TranslateSBoolLiteralExpToMExp(exp, context));
    }

    ResultType Visit(SExp_NullLiteral* exp)
    {
        return HandleExp(TranslateSNullLiteralExpToMExp(exp, hintType, context));
    }

    ResultType Visit(SExp_BinaryOp* exp)
    {
        return HandleExp(TranslateSBinaryOpExpToMExp(exp, context));
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
            return HandleExp(TranslateSUnaryOpExpToMExpExceptDeref(exp, context));
        }
    }

    ResultType Visit(SExp_Call* exp)
    {
        return HandleExp(TranslateSCallExpToMExp(exp, hintType, context));
    }

    ResultType Visit(SExp_Lambda* exp)
    {
        return HandleExp(TranslateSLambdaExpToMExp(exp, context));
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
        return HandleExp(TranslateSListExpToMExp(exp, context));
    }

    // 'new C(...)'
    ResultType Visit(SExp_New* exp)
    {
        return HandleExp(TranslateSNewExpToMExp(exp, context));
    }

    ResultType Visit(SExp_Box* exp)
    {
        return HandleExp(TranslateSBoxExpToMExp(exp, hintType, context));
    }

    ResultType Visit(SExp_Is* exp)
    {
        return HandleExp(TranslateSIsExpToMExp(exp, context));
    }

    ResultType Visit(SExp_As* exp)
    {
        return HandleExp(TranslateSAsExpToMExp(exp, context));
    }
};

} // namespace

expected<ReExp*, DiagPtr> TranslateSExpToReExp(SExp* exp, RType* hintType, TranslationContext& context)
{
    SExpToReExpTranslator translator{hintType, context};
    return Accept(translator, exp);
}

} // namesapce Citron::SyntaxIR0Translator