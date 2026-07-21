#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "ImplRFuncDeclUsingCommonComponents.h"

namespace Citron {

class RImplTraitDecl;

class RImplTraitFuncDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents<RImplTraitFuncDecl>
{
    RImplTraitDecl* implTrait;
    RTraitFuncDecl* traitFunc; // correspoding trait func

    RGenericsComponent genericsComp;
    RCommonFuncDeclComponent commonFuncDeclComp;

public:
    RImplTraitFuncDecl(RImplTraitDecl* implTrait);

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