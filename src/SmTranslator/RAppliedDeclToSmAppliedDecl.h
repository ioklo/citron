#pragma once
#include <expected>
#include <vector>
#include "Logging/Diag.h"
#include "RSymbol/RAppliedDecl.h"
#include "RSymbol/RTypeArguments.h"
#include "SmAppliedDecl.h"

namespace Citron {

class SmFactory;

// env 없이 리턴한다
template<typename TRDecl>
SmAppliedDecl<TRDecl> TranslateRAppliedDeclToSmAppliedDecl(RAppliedDecl<TRDecl>& rAppliedDecl, SmFactory* factory)
{
    std::vector<SmType*> typeArgs;

    size_t count = rAppliedDecl.typeArgs->GetCount();
    typeArgs.reserve(count);
    for (size_t i = 0; i < count; i++)
    {
        auto* rTypeArg = rAppliedDecl.typeArgs->Get(i);
        auto e_smType = TranslateRTypeToSmType(rTypeArg, factory);
        RETURN_ON_ERROR(e_smType);

        typeArgs.push_back(std::move(*e_smType));
    }
    
    return SmAppliedDecl<TRDecl>{rAppliedDecl.decl, std::move(typeArgs)};
}

} // namespace Citron