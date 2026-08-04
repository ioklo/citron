#include "STypeExpToImTypeExp.h"
#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RFactory.h"
#include "ImTypeExp.h"
#include "SmTypeTranslation.h"
#include "SmTypeTranslationContexts.h"
#include "STypeExp_IdToImTypeExp.h"
#include "STypeExp_MemberToImTypeExp.h"

using namespace std;

namespace Citron {

expected<ImTypeExp, DiagPtr> TranslateSTypeExpToImTypeExp(STypeExp* sTypeExp, SmTypeTranslationContexts& contexts)
{
    struct Translator
    {
        using ResultType = expected<ImTypeExp, DiagPtr>;

        SmTypeTranslationContexts& contexts;

        ResultType Visit(STypeExp_Id* typeExp) { return TranslateSTypeExp_IdToImTypeExp(typeExp, contexts); }
        ResultType Visit(STypeExp_Member* typeExp) { return TranslateSTypeExp_MemberToImTypeExp(typeExp, contexts); }
        ResultType Visit(STypeExp_Nullable* typeExp) 
        {
            auto e_innerType = TranslateSTypeExpToRType(typeExp->innerType, contexts);
            RETURN_ON_ERROR(e_innerType);

            auto* type = contexts.rFactory->MakeNullableType(*e_innerType);
            return ImTypeExp_Type{type};
        }

        ResultType Visit(STypeExp_Shared* typeExp) 
        {
            auto e_innerType = TranslateSTypeExpToRType(typeExp->innerType, contexts);
            RETURN_ON_ERROR(e_innerType);

            auto* type = contexts.rFactory->MakeSharedType(*e_innerType);
            return ImTypeExp_Type{type};
        }

        ResultType Visit(STypeExp_Box* typeExp) 
        {
            auto e_innerType = TranslateSTypeExpToRType(typeExp->innerType, contexts);
            RETURN_ON_ERROR(e_innerType);

            auto* type = contexts.rFactory->MakeBoxType(*e_innerType);
            return ImTypeExp_Type{type};
        }

        ResultType Visit(STypeExp_Ptr* typeExp) 
        {
            auto e_innerType = TranslateSTypeExpToRType(typeExp->innerType, contexts);
            RETURN_ON_ERROR(e_innerType);

            auto* type = contexts.rFactory->MakePtrType(*e_innerType);
            return ImTypeExp_Type{type};
        }

        ResultType Visit(STypeExp_Local* typeExp) 
        {
            // TODO: [75] 2020-08-04, local type 구현
            throw NotImplementedException{};

            /*auto e_innerType = TranslateSTypeExpToRType(typeExp->innerType, contexts);
            RETURN_ON_ERROR(e_innerType);

            auto* type = contexts.rFactory->MakeLocal(*e_innerType);
            return ImTypeExp_Type{type};*/
        }
    };

    return Accept(Translator{contexts}, sTypeExp);
}

} // namespace Citron