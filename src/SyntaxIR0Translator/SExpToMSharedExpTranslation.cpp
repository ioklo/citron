#include "SExpToMSharedExpTranslation.h"

#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "Logging/Diag.h"
#include "RSymbol/RFactory.h"

#include "TranslationContexts.h"
#include "Misc.h"
#include "SExpToIrExpTranslation.h"
#include "IrExpAndMemberNameToMSharedExpTranslation.h"

using namespace std;

namespace Citron {

struct SExpToMSharedExpTranslator
{
    using ResultType = expected<MSharedExp*, DiagPtr>;
    TranslationContexts& contexts;

    ResultType Visit(SExp* sExp)
    {
        return Error<Error_SharedTranslation_SingleRefNotAllowed>();
    }

    // ResultType Visit(SExp_Identifier* sExp);
    // ResultType Visit(SExp_String* sExp);
    // ResultType Visit(SExp_IntLiteral* sExp);
    // ResultType Visit(SExp_BoolLiteral* sExp);
    // ResultType Visit(SExp_NullLiteral* sExp);
    // ResultType Visit(SExp_BinaryOp* sExp);
    // ResultType Visit(SExp_UnaryOp* sExp);
    // ResultType Visit(SExp_Call* sExp);
    // ResultType Visit(SExp_Lambda* sExp);
    // ResultType Visit(SExp_Indexer* sExp);

    ResultType Visit(SExp_Member* sExp) 
    {
        auto e_irBaseExp = TranslateSExpToIrExp(sExp->base, contexts);
        RETURN_ON_ERROR(e_irBaseExp);

        auto e_typeArgs = MakeRTypeArgs(sExp->memberTypeArgs, contexts);
        RETURN_ON_ERROR(e_typeArgs);

        return TranslateIrExpAndMemberNameToMSharedExp(*e_irBaseExp, RName_Normal{sExp->memberName}, *e_typeArgs, contexts);
    }

    // ResultType Visit(SExp_IndirectMember* sExp) { }
    // ResultType Visit(SExp_List* sExp) { }
    // ResultType Visit(SExp_New* sExp) { }
    // ResultType Visit(SExp_Box* sExp) { }
    // ResultType Visit(SExp_Is* sExp) { }
    // ResultType Visit(SExp_As* sExp) { }
};


expected<MSharedExp*, DiagPtr> TranslateSExpToMSharedExp(SExp* sExp, TranslationContexts& contexts)
{

}

} // namespace Citron