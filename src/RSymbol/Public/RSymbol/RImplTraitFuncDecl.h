#pragma once
#include "RSymbolConfig.h"
#include <optional>
#include "RDecl.h"
#include "ImplRFuncDeclUsingCommonComponents.h"
#include "RGenericsComponent.h"
#include "RCommonFuncDeclComponent.h"
#include "RDeclKey.h"

namespace Citron {

class RImplTraitDecl;

class RImplTraitFuncDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents<RImplTraitFuncDecl>
{
    std::optional<RDeclKey> o_key;
    RImplTraitDecl* implTrait;
    RName name;
    RTraitFuncDecl* traitFunc; // correspoding trait func

    RGenericsComponent genericsComp;
    RCommonFuncDeclComponent commonFuncDeclComp;

public:
    RSYMBOL_API RImplTraitFuncDecl(RImplTraitDecl* implTrait, bool bSeqFunc, RName&& name);
    RSYMBOL_API void Init(RDeclKey&& key, std::vector<RTypeParam*>&& typeParams, RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams);

public:
    // from RDecl
    RSYMBOL_API RDeclKey& GetDeclKey() final;
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RName* TryGetName() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;
};

} // namespace Citron