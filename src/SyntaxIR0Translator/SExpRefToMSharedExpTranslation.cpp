#include "SExpRefToMSharedExpTranslation.h"

#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"
#include "Logging/Diag.h"
#include "Syntax/Syntaxes.g.h"
#include "SExpRefToIrExpTranslation.h"
#include "IrExpAndMemberNameToMSharedExpTranslation.h"
#include "Misc.h"

using namespace std;

namespace Citron {

struct SExpRefToMSharedExpTranslator
{
    using ResultType = expected<MSharedExp*, DiagPtr>;
    TranslationContexts& contexts;

    template<typename TError, typename... TArgs>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TError>(forward<TArgs>(args)...)};
    }
    
    // shared<int> i = &c;
    // shared<int> i = &&c; // 원래 불가인데, 단일 &는 허용 안되기 때문에, 따로 처리하지 않아도 된다
    ResultType Visit(SExp* exp)
    {   
        return Error<Error_SharedRefTranslation_DirectRefNotAllowed>();
    }

    ResultType Visit(SExp_Member* exp)
    {
        auto e_irParent = TranslateSExpRefToIrExp(exp->parent, contexts);
        RETURN_ON_ERROR(e_irParent);

        auto e_rTypeArgsExceptOuter = MakeRTypeArgs(exp->memberTypeArgs, contexts);
        RETURN_ON_ERROR(e_rTypeArgsExceptOuter);

        // 합체
        return TranslateIrExpAndMemberNameToMSharedExp(*e_irParent, RName_Normal(exp->memberName), *e_rTypeArgsExceptOuter, contexts);
    }

    ResultType Visit(SExp_IndirectMember* exp)
    {
        throw NotImplementedException{};
    }
};

expected<MSharedExp*, DiagPtr> TranslateSExpRefToMSharedExp(SExp* sExp, TranslationContexts& contexts)
{
    return Accept(SExpRefToMSharedExpTranslator{contexts}, sExp);
}

} // Citron