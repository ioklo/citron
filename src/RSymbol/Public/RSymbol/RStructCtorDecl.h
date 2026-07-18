#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "RFuncDecl.h"
#include "RGenericsComponent.h"
#include "RCommonFuncDeclComponent.h"
#include "ImplRFuncDeclUsingCommonComponents.h"

namespace Citron {

enum class RStructMemberAccessor;

enum class RStructCtorKind
{
    Normal,
    Memberwise,
    Copy,
    Move,
};

class RStructCtorDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents<RStructCtorDecl>
{
    RStructDecl* _struct;
    RStructMemberAccessor accessor;
    RStructCtorKind kind;

    RGenericsComponent genericsComp;
    RCommonFuncDeclComponent commonFuncDeclComp;

public:
    RSYMBOL_API RStructCtorDecl(RStructDecl* _struct, RStructMemberAccessor accessor, RStructCtorKind kind);
    RSYMBOL_API void InitFuncParameters(std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    RSYMBOL_API void InitTypeParams(std::vector<RTypeParam*>&& typeParams);

    RStructDecl* GetStructDecl() { return _struct; }
    RStructMemberAccessor GetAccessor() { return accessor; }
    RStructCtorKind GetKind() { return kind; }

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
