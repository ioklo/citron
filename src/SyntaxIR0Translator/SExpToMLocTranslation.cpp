#include "SExpToMLocTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Logging/Logger.h"
#include "Syntax/Syntax.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"

#include "DesignatedDiagnostic.h"
#include "ReExpToMLocTranslation.h"
#include "SExpToReExpTranslation.h"
#include "SExpToMExpTranslation.h"

#include "TranslationContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

class SExpToMLocTranslator
{
public:
    using ResultType = expected<MLoc*, DiagPtr>;

private:
    RType* hintType;
    bool bWrapExpAsLoc;

    IDesignatedDiagnostic* notLocationDiag;
    TranslationContext& context;

public:
    SExpToMLocTranslator(
        RType* hintType,
        bool bWrapExpAsLoc,
        IDesignatedDiagnostic* notLocationDiag,
        TranslationContext& context)
        : hintType{hintType}, bWrapExpAsLoc{bWrapExpAsLoc}, notLocationDiag{notLocationDiag}, context{context}
    {
    }

private:
    ResultType HandleDefault(SExp* sExp)
    {
        if (auto eReExp = TranslateSExpToReExp(sExp, hintType, context))
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            return TranslateReExpToMLoc(*eReExp, bWrapExpAsLoc, &designatedDiag, context);
        }
        else // invalid
        {
            return unexpected{move(eReExp).error()};
        }
    }

    // fast track
    ResultType HandleExp(expected<MExp*, DiagPtr>&& eNExp)
    {
        if (!eNExp)
        {
            return unexpected{move(eNExp).error()};
        }
        else if (bWrapExpAsLoc)
        {
            return context.MakeNLoc<MLoc_Temp>(*eNExp);
        }
        else
        {
            return unexpected{notLocationDiag->MakeDiag()};
        }
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

public:
    ResultType Visit(SExp_Identifier* exp)
    {
        return HandleDefault(exp);
    }

    ResultType Visit(SExp_String* exp)
    {
        auto eNExp = TranslateSStringExpToNStringExp(exp, context);
        if (!eNExp) return Error(move(eNExp));

        return HandleExp(move(*eNExp));
    }

    ResultType Visit(SExp_IntLiteral* exp)
    {
        auto eNExp = TranslateSIntLiteralExpToMExp(exp, context);
        if (!eNExp) return Error(move(eNExp));

        return HandleExp(move(*eNExp));
    }

    ResultType Visit(SExp_BoolLiteral* exp)
    {
        auto eNExp = TranslateSBoolLiteralExpToMExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    ResultType Visit(SExp_NullLiteral* exp)
    {
        auto eNExp = TranslateSNullLiteralExpToMExp(exp, hintType, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    ResultType Visit(SExp_BinaryOp* exp)
    {
        auto eNExp = TranslateSBinaryOpExpToMExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
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
            auto eNExp = TranslateSUnaryOpExpToMExpExceptDeref(exp, context);
            if (!eNExp) return Error(move(eNExp));
            return HandleExp(move(*eNExp));
        }
    }

    ResultType Visit(SExp_Call* exp)
    {
        auto eNExp = TranslateSCallExpToMExp(exp, hintType, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    ResultType Visit(SExp_Lambda* exp)
    {
        auto eNExp = TranslateSLambdaExpToMExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
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
        auto eNExp = TranslateSListExpToMExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    ResultType Visit(SExp_New* exp)
    {
        auto eNExp = TranslateSNewExpToMExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    ResultType Visit(SExp_Box* exp)
    {
        auto eNExp = TranslateSBoxExpToMExp(exp, hintType, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    ResultType Visit(SExp_Is* exp)
    {
        auto eNExp = TranslateSIsExpToMExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }

    ResultType Visit(SExp_As* exp)
    {
        auto eNExp = TranslateSAsExpToMExp(exp, context);
        if (!eNExp) return Error(move(eNExp));
        return HandleExp(move(*eNExp));
    }
};

} // namespace 

expected<MLoc*, DiagPtr> TranslateSExpToMLoc(SExp* sExp, RType* hintType, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context)
{
    SExpToMLocTranslator translator{hintType, bWrapExpAsLoc, notLocationDiag, context};
    return Accept(translator, sExp);
}


} // namespace Citron::SyntaxIR0Translator
