#pragma once
#include "RSymbolConfig.h"
#include "RDecl.h"
#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "RGenericsComponent.h"
#include "RDeclKey.h"

namespace Citron {

class RTraitDecl;

class RTraitFuncDecl : public RDecl
{
    struct LazyInit
    {
        RDeclKey key;
        std::vector<RTypeParam*> typeParams;
        RFuncReturn funcReturn;
        std::vector<RFuncParameter> funcParameters;
        bool bLastParamVariadic;
    };

    std::optional<LazyInit> o_lazyInit;
    RTraitDecl* trait;
    bool bStatic;
    RName name;

    RGenericsComponent genericsComp;

public:
    RSYMBOL_API RTraitFuncDecl(RTraitDecl* trait, bool bStatic, RName&& name);
    RSYMBOL_API void Init(RDeclKey&& key, std::vector<RTypeParam*>&& typeParams, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParamVariadic);

    RFuncReturn GetUnboundFuncReturn() { return o_lazyInit->funcReturn; }
    std::span<RFuncParameter> GetUnboundFuncParameters() { return o_lazyInit->funcParameters; }

public: // from RDecl
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