#include "SExpToMOperandTranslation.h"

#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "SExpToReExpTranslation.h"
#include "ReExpToMOperandTranslation.h"
#include "SExpToMExpTranslation.h"

#include "MIR/MExp.h"

using namespace std;

namespace Citron {

namespace {

struct SExpToMOperandTranslator
{
    using ResultType = expected<MOperand, DiagPtr>;

    RType* hintType;
    TranslationContexts& contexts;

    SExpToMOperandTranslator(RType* hintType, TranslationContexts& contexts)
        : hintType{hintType}, contexts{contexts}
    {
    }

    ResultType HandleDefault(SExp* sExp)
    {
        auto e_reExp = TranslateSExpToReExp(sExp, hintType, contexts);
        RETURN_ON_ERROR(e_reExp);

        return TranslateReExpToMOperand(*e_reExp, contexts);
    }

    // fast track
    template<typename TMExp> requires derived_from<TMExp, MExp>
    ResultType HandleExp(expected<TMExp*, DiagPtr>&& e_mExp)
    {
        RETURN_ON_ERROR(e_mExp);
        return MOperand_Exp{*e_mExp};
    }

    template<typename TValue>
    ResultType Error(expected<TValue, DiagPtr>&& e)
    {
        return unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

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

    ResultType Visit(SExp_UnaryOp* exp)
    {
        // Deref는 loc으로 변경되어야 한다
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

    ResultType Visit(SExp_Member* exp)
    {
        return HandleDefault(exp);
    }

    // s->x
    ResultType Visit(SExp_IndirectMember* exp)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(SExp_List* exp)
    {
        return HandleExp(TranslateSListExpToMExp(exp, contexts));
    }

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


expected<MOperand, DiagPtr> TranslateSExpToMOperand(SExp* exp, RType* hintType, TranslationContexts& contexts)
{
    SExpToMOperandTranslator translator{hintType, contexts};
    return Accept(translator, exp);
}

} // namespace Citron