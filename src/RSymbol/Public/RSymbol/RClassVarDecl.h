#pragma once
#include "RSymbolConfig.h"
#include "RDecl.h"

namespace Citron {

class RType;
class RClassDecl;
class RTypeArguments;

class RClassVarDecl final : public RDecl
{
    RClassDecl* _class;

    RClassMemberAccessor accessor;
    bool bStatic;
    RType* declType;
    RName name;

public:
    RSYMBOL_API RClassVarDecl(RClassDecl* _class, RClassMemberAccessor accessor, bool bStatic, RType* declType, TakeRef<RName> name);

    RType* GetUnboundDeclType() { return declType; }
    bool IsStatic() { return bStatic; }

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
