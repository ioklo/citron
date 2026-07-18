#pragma once
#include "RSymbolConfig.h"
#include "RDecl.h"
#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "RGenericsComponent.h"

namespace Citron {

class RTraitDecl;

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
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RIdentifier GetIdentifier() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;
};

} // namespace Citron