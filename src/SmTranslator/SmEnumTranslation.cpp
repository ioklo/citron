#include "SmEnumTranslation.h"
#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RTypeDeclOuter.h"
#include "RSymbol/REnumDecl.h"
#include "RSymbol/REnumElemDecl.h"
#include "RSymbol/REnumElemVarDecl.h"
#include "RSymbol/RDeclKey.h"
#include "RSymbol/RFactory.h"
#include "CommonTranslation.h"
#include "SmDeclContext_Decl.h"
#include "SmEnumElemVarTranslation.h"

using namespace std;

namespace Citron {

// body를 만들지 않으므로 declContext를 만들지 않는다
void SmTranslateEnum(InRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, SEnumDecl* sEnum, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    RName enumName{RName::Normal(sEnum->name)};

    auto* rEnum = (*rFactory)->MakeDecl<REnumDecl>(RDeclKey::Normal(enumName), outer, move(enumName), *rFactory);
    auto typeParams = MakeTypeParams(outer.GetDecl()->GetAllTypeParamCount(), rEnum, sEnum->typeParams, *rFactory);
    rEnum->InitTypeParams(move(typeParams));
    outer.AddType(rEnum);

    SmDeclContextPtr enumDeclContext = MakePtr<SmDeclContext_Decl<REnumDecl>>(*outerDeclContext, rEnum, rEnum->MakeOpenTypeArgs(**rFactory));
    auto* emptyTypeArgs = (*rFactory)->MakeEmptyTypeArguments();

    // EnumElem
    for (auto* sEnumElem : sEnum->elements)
    {
        RName elemName{RName::Normal(sEnumElem->name)};
        auto* rEnumElem = (*rFactory)->MakeDecl<REnumElemDecl>(RDeclKey::Normal(elemName), rEnum, move(elemName), *rFactory);
        rEnum->AddElem(rEnumElem);

        SmDeclContextPtr enumElemDeclContext = MakePtr<SmDeclContext_Decl<REnumElemDecl>>(enumDeclContext, rEnumElem, emptyTypeArgs);

        // EnumElemVar
        for (auto* sEnumElemVar : sEnumElem->vars)
        {   
            SmTranslateEnumElemVar(enumElemDeclContext, sEnumElemVar, rEnumElem, rFactory, phaseManager);
        }
    }
}



} // namespace Citron