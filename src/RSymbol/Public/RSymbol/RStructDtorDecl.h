#pragma once
#include "RSymbolConfig.h"
#include "RDecl.h"
#include "ImplRFuncDeclUsingCommonComponents.h"
#include "RCommonFuncDeclComponent.h"
#include "RDeclKey.h"

namespace Citron {

class RStructDecl;
enum class RStructMemberAccessor;

class RStructDtorDecl final : public RDecl, public ImplRFuncDeclUsingCommonComponents<RStructDtorDecl>
{
    RDeclKey key;
    RStructDecl* _struct;
    RStructMemberAccessor accessor;
    
    RCommonFuncDeclComponent commonFuncDeclComp;

public:
    RSYMBOL_API RStructDtorDecl(RDeclKey&& key, RStructDecl* _struct, RStructMemberAccessor accessor);
    RStructDecl* GetStructDecl() { return _struct; }
    RStructMemberAccessor GetAccessor() { return accessor; }

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