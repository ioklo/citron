#pragma once

#include "RSymbolConfig.h"
#include "RDecl.h"
#include "RDeclKey.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"

namespace Citron {

class RType;

class RTypeAliasDecl final : public RDecl, public RTypeDecl
{
    RDeclKey key;
    RTypeDeclOuter outer;
    RName name;
    RType* targetType{};

public:
    RSYMBOL_API RTypeAliasDecl(RDeclKey&& key, RTypeDeclOuter outer, RName&& name);
    RSYMBOL_API void InitTargetType(RType* targetType);
    RSYMBOL_API RType* GetTargetType();

public: // from RDecl
    RSYMBOL_API RDeclKey& GetDeclKey() final;
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RName* TryGetName() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;

public: // from RTypeDecl
    RSYMBOL_API RDecl* RTypeDecl_GetDecl() final;
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
};

} // namespace Citron
