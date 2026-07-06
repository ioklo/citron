#pragma once
#include "RSymbolConfig.h"
#include "RDecl.h"
#include "ImplRFuncDeclUsingCommonComponents.h"
#include "RCommonFuncDeclComponent.h"

namespace Citron {

class RStructDecl;
enum class RStructMemberAccessor;

class RStructDtorDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents
{
    RStructDecl* _struct;
    RStructMemberAccessor accessor;
    
    RCommonFuncDeclComponent commonFuncDeclComp;

public:
    RSYMBOL_API RStructDtorDecl(RStructDecl* _struct, RStructMemberAccessor accessor);
    RStructDecl* GetStructDecl() { return _struct; }
    RStructMemberAccessor GetAccessor() { return accessor; }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) override;
    RSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron