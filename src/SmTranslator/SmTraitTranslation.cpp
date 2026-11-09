#include "SmTraitTranslation.h"
#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RTypeDeclOuter.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RDeclKey.h"
#include "RSymbol/RTraitDecl.h"
#include "RSymbol/RFactory.h"
#include "CommonTranslation.h"
#include "SmDeclContext_Decl.h"
#include "SmTraitTypeTranslation.h"
#include "SmTraitFuncTranslation.h"

using namespace std;

namespace Citron {

void SmTranslateTrait(InRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, STraitDecl* sTrait, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    RName traitName{RName::Normal(sTrait->name)};
    auto* rTraitDecl = (*rFactory)->MakeDecl<RTraitDecl>(RDeclKey::Normal(traitName), outer, move(traitName), *rFactory);

    auto typeParams = MakeTypeParams(outer.GetDecl()->GetAllTypeParamCount(), rTraitDecl, sTrait->typeParams, *rFactory);
    rTraitDecl->InitTypeParams(move(typeParams));
    outer.AddType(rTraitDecl);

    SmDeclContextPtr traitDeclContext = MakePtr<SmDeclContext_Decl<RTraitDecl>>(*outerDeclContext, rTraitDecl, rTraitDecl->MakeOpenTypeArgs(**rFactory));

    for (auto& sMemberDecl : sTrait->memberDecls)
    {
        visit([&traitDeclContext, rTraitDecl, &rFactory, &phaseManager](auto* sMemberDecl) {
            using T = remove_cvref_t<decltype(sMemberDecl)>;

            if constexpr (same_as<T, STraitTypeDecl*>)
            {
                SmTranslateTraitType(traitDeclContext, sMemberDecl, rTraitDecl, rFactory, phaseManager);
            }
            else if constexpr (same_as<T, STraitFuncDecl*>)
            {
                SmTranslateTraitFunc(traitDeclContext, rTraitDecl, sMemberDecl, rFactory, phaseManager);
            }
            else static_assert(false);

        }, sMemberDecl);
    }
}

} // namespace Citron