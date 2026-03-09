#include "ReExpToMCreateTranslation.h"
#include <cassert>
#include "RSymbol/RTypes.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"
#include "MIR/MInitExp.h"
#include "MIR/MFactory.h"
#include "ReExp.h"
#include "TranslationContexts.h"

using namespace std;

namespace Citron {

expected<MCreate, DiagPtr> TranslateReExpToMCreate(ReExp* reExp, TranslationContexts& contexts)
{
    struct Visitor
    {
        using ResultType = expected<MCreate, DiagPtr>;
        TranslationContexts& contexts;

        ResultType Visit(ReExp_Loc* reExp) 
        {   
            auto* type = GetType(reExp, &*contexts.rFactory);
            
            if (type->IsBitwiseCopyable()) // BC
            {   
                auto* mExp = contexts.mFactory->MakeMExp<MExp_Load>(MRead_Location{reExp->mLoc});
                return MCreate_Bitwise{mExp};
            }
            else // NBC
            {
                if (auto* structType = dynamic_cast<RType_Struct*>(type))
                {
                    auto* mInitExp = contexts.mFactory->MakeMInitExp<MInitExp_StructCtor>(MInitExp_StructCtorKind_Copy{MRead_Location{reExp->mLoc}});
                    return MCreate_Init{mInitExp};
                }
                else throw NotImplementedException{};
            }
        }

        ResultType Visit(ReExp_Exp* reExp) 
        {
            assert(GetType(reExp, &*contexts.rFactory)->IsBitwiseCopyable());

            return MCreate_Bitwise{reExp->mExp};
        }

        ResultType Visit(ReExp_InitExp* reExp) 
        {
            assert(!GetType(reExp, &*contexts.rFactory)->IsBitwiseCopyable());

            return MCreate_Init{reExp->mInitExp};
        }
    };

    return Accept(Visitor{contexts}, reExp);
}

} // namespace Citron