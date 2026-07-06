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

class RStructCtorDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents
{
    RStructDecl* _struct;
    RStructMemberAccessor accessor;
    RStructCtorKind kind;

    RGenericsComponent genericsComp;
    RCommonFuncDeclComponent commonFuncDeclComp;

public:
    RSYMBOL_API RStructCtorDecl(RStructDecl* _struct, RStructMemberAccessor accessor, RStructCtorKind kind);

    RStructDecl* GetStructDecl() { return _struct; }
    RStructMemberAccessor GetAccessor() { return accessor; }
    RStructCtorKind GetKind() { return kind; }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RIdentifier GetIdentifier() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) final;
    RSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) final;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) final;
};

} // namespace Citron
