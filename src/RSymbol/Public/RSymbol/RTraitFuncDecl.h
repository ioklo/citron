#pragma once
#include "RSymbolConfig.h"
#include "RDecl.h"
#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "RGenericsComponent.h"

namespace Citron {

class RTraitFuncDecl : public RDecl
{
    RTraitDecl* trait;

    bool bStatic;
    RFuncReturn funcReturn;
    RName name;
    std::vector<RFuncParameter> funcParameters;
    bool bLastParamVariadic;

    RGenericsComponent genericsComp;

public:
    RSYMBOL_API RTraitFuncDecl(RTraitDecl* trait, bool bStatic, RFuncReturn&& funcReturn, RName&& name, std::vector<RFuncParameter>&& funcParameters, bool bLastParamVariadic);

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron