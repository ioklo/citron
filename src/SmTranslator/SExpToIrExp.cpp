#include "SExpToIrExp.h"

#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RTypeArguments.h"
#include "MIR/MLoc.h"

#include "IrExp.h"
#include "SRTFactory.h"
#include "Misc.h"
#include "TranslationContexts.h"
#include "SExp_IdentifierToIrExp.h"
#include "SExp_MemberToIrExp.h"
#include "DesignatedDiagnostic.h"
#include "SExpTranslations.h"

using namespace std;

namespace Citron {

namespace {

// SExp_Member의 base부분에 대한 translator
struct SExpToIrExpTranslator
{
    using ResultType = expected<IrExp*, DiagPtr>;
    TranslationContexts& contexts;

    ResultType HandleDefault(SExp* exp)
    {
        DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> notLocationDiag;
        auto e_loc = TranslateSExpToMLoc(exp, /*hintType*/nullptr, /*bMaterializeExp*/true, &notLocationDiag, contexts);
        RETURN_ON_ERROR(e_loc);

        return contexts.srtFactory->MakeIrExp<IrExp_Loc>(*e_loc);
    }

    // 기본
    ResultType Visit(SExp* exp)
    {
        return HandleDefault(exp);
    }

    ResultType Visit(SExp_Identifier* exp)
    {
        return TranslateSExp_IdentifierToIrExp(exp, contexts);
    }

    // Loc으로 최대한 만들어서, binding시점, MSharedExp변환 시점에 터트린다
    // ResultType Visit(SExp_String* exp); "abc"
    // ResultType Visit(SExp_IntLiteral* exp); 1
    // ResultType Visit(SExp_BoolLiteral* exp); true
    // ResultType Visit(SExp_NullLiteral* exp); null
    // ResultType Visit(SExp_BinaryOp* exp); // &(e0 + e1).id, &(e0 = e1).id

    optional<IrExp*> TrySharedDeref(SExp* operand)
    {
        // x가 shared<S>인 경우에만 IrExp_SharedDeref로 바꿔서 보존한다
        DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> notLocationDiag;

        auto e_mRead = TranslateSExpToMRead(operand, /*hintType*/nullptr, contexts);
        if (!e_mRead) return nullopt;

        auto* mReadLoc = get_if<MRead_Loc>(&*e_mRead);
        if (!mReadLoc) return nullopt;

        auto* targetType = GetType(mReadLoc->loc, &*contexts.rFactory);

        // shared<S> 꼴인지 확인
        if (auto* targetSharedType = dynamic_cast<RType_Shared*>(targetType))
            if (dynamic_cast<RType_Struct*>(targetSharedType->innerType))
                return contexts.srtFactory->MakeIrExp<IrExp_SharedDeref>(move(*mReadLoc));

        return nullopt;
    }

    // *x
    ResultType Visit(SExp_UnaryOp* exp)
    {
        if (exp->kind == SUnaryOpKind::Deref)
            if (auto o_irExp = TrySharedDeref(exp->operand))
                return *o_irExp;

        return HandleDefault(exp);
    }

    // ResultType Visit(SExp_Call* exp);
    // ResultType Visit(SExp_Lambda* exp);
    // ResultType Visit(SExp_Indexer* exp);
    ResultType Visit(SExp_Member* exp)
    {
        return TranslateSExp_MemberToIrExp(exp, contexts);
    }
    //ResultType Visit(SExp_List* exp);
    //ResultType Visit(SExp_New* exp);
    //ResultType Visit(SExp_Shared* exp);
    //ResultType Visit(SExp_Is* exp);
    //ResultType Visit(SExp_As* exp);
};

} // namespace 

expected<IrExp*, DiagPtr> TranslateSExpToIrExp(SExp* sExp, TranslationContexts& contexts)
{   
    return Accept(SExpToIrExpTranslator{contexts}, sExp);
}

} // namespace Citron