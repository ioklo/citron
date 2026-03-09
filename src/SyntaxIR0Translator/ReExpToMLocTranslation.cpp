#include "ReExpToMLocTranslation.h"
#include <cassert>
#include "Infra/Ptr.h"
#include "Infra/Expected.h"
#include "Logging/Logger.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NLambdaVarDecl.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "ScopeContext.h"
#include "DesignatedDiagnostic.h"
#include "ReExp.h"
#include "TranslationContexts.h"
#include "FuncContext.h"

using namespace std;

namespace Citron {

expected<MLoc*, DiagPtr> TranslateReExpToMLoc(ReExp* reExp, bool bMaterializeExp, IDesignatedDiagnostic* notLocationDiag, TranslationContexts& contexts)
{
    struct Visitor
    {
        using ResultType = expected<MLoc*, DiagPtr>;
        bool bMaterializeExp;
        IDesignatedDiagnostic* notLocationDiag;
        TranslationContexts& contexts;

        ResultType Visit(ReExp_Loc* reExp) 
        {
            return reExp->mLoc;
        }

        ResultType Visit(ReExp_Exp* reExp) 
        {
            assert(GetType(reExp, &*contexts.rFactory)->IsBitwiseCopyable());
            if (!bMaterializeExp) return unexpected{notLocationDiag->MakeDiag()};
            return contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_Bitwise{reExp->mExp});
        }

        ResultType Visit(ReExp_InitExp* reExp) 
        {
            assert(!GetType(reExp, &*contexts.rFactory)->IsBitwiseCopyable());
            if (!bMaterializeExp) return unexpected{notLocationDiag->MakeDiag()};
            return contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_Init{reExp->mInitExp});
        }
    };

    return Accept(Visitor{bMaterializeExp, notLocationDiag, contexts}, reExp);
}

}
