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

    ResultType HandleExp(expected<NExp*, DiagPtr>&& eExp)
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
        return HandleExp(TranslateSIntLiteralExpToNExp(exp, context));
    }

    ResultType Visit(SExp_BoolLiteral* exp)
    {
        return HandleExp(TranslateSBoolLiteralExpToNExp(exp, context));
    }

    ResultType Visit(SExp_NullLiteral* exp)
    {
        return HandleExp(TranslateSNullLiteralExpToNExp(exp, hintType, context));
    }

    ResultType Visit(SExp_BinaryOp* exp)
    {
        return HandleExp(TranslateSBinaryOpExpToNExp(exp, context));
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
            return HandleExp(TranslateSUnaryOpExpToNExpExceptDeref(exp, context));
        }
    }

    ResultType Visit(SExp_Call* exp)
    {
        return HandleExp(TranslateSCallExpToNExp(exp, hintType, context));
    }

    ResultType Visit(SExp_Lambda* exp)
    {
        return HandleExp(TranslateSLambdaExpToNExp(exp, context));
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
        return HandleExp(TranslateSListExpToNExp(exp, context));
    }

    // 'new C(...)'
    ResultType Visit(SExp_New* exp)
    {
        return HandleExp(TranslateSNewExpToNExp(exp, context));
    }

    ResultType Visit(SExp_Box* exp)
    {
        return HandleExp(TranslateSBoxExpToNExp(exp, hintType, context));
    }

    ResultType Visit(SExp_Is* exp)
    {
        return HandleExp(TranslateSIsExpToNExp(exp, context));
    }

    ResultType Visit(SExp_As* exp)
    {
        return HandleExp(TranslateSAsExpToNExp(exp, context));
    }
};

} // namespace

expected<ReExp*, DiagPtr> TranslateSExpToReExp(SExp* exp, RType* hintType, TranslationContext& context)
{
    SExpToReExpTranslator translator{hintType, context};
    return Accept(translator, exp);
}

} // namesapce Citron::SyntaxIR0Translator