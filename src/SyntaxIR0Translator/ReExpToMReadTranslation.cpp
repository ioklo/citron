#include "ReExpToMReadTranslation.h"
#include <cassert>
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "RSymbol/RTypes.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"
#include "MIR/MFactory.h"
#include "ReExp.h"
#include "ReExpToMLocTranslation.h"
#include "TranslationContexts.h"

using namespace std;

namespace Citron {

expected<MRead, DiagPtr> TranslateReExpToMRead(ReExp* reExp, TranslationContexts& contexts)
{
    struct Visitor
    {
        using ResultType = expected<MRead, DiagPtr>;
        TranslationContexts& contexts;

        ResultType Visit(ReExp_Loc* reExp) 
        {
            auto* type = GetType(reExp, &*contexts.rFactory);

            if (type->IsBitwiseCopyable())
            {
                auto* mExp = contexts.mFactory->MakeMExp<MExp_Load>(reExp->mLoc);
                return MRead_Value{mExp};
            }
            else
            {
                return MRead_Location{reExp->mLoc};
            }
        }

        ResultType Visit(ReExp_Exp* reExp) 
        {
            assert(GetType(reExp, &*contexts.rFactory)->IsBitwiseCopyable());

            return MRead_Value{reExp->mExp};
        }

        ResultType Visit(ReExp_InitExp* reExp) 
        {
            assert(!GetType(reExp, &*contexts.rFactory)->IsBitwiseCopyable());

            auto* mLoc = contexts.mFactory->MakeMLoc<MLoc_Materialize>(MCreate_Init{reExp->mInitExp});
            return MRead_Location{mLoc};
        }
    };

    return Accept(Visitor{contexts}, reExp);
}

} // namespace Citron