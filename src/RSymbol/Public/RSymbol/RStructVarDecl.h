#pragma once

#include "RDecl.h"

namespace Citron {

class RType;
class RStructDecl;
class RFactory;

class RStructVarDecl final : public RDecl
{
    RStructDecl* _struct;
    RStructMemberAccessor accessor;
    bool bStatic;
    RType* declType;
    RName name;
    size_t index;

public:
    RSYMBOL_API RStructVarDecl(RStructDecl* _struct, RStructMemberAccessor accessor, bool bStatic, RType* declType, TakeRef<RName> name, size_t index);

    bool IsStatic() { return bStatic; }
    RType* GetUnboundDeclType() { assert(declType); return declType; }
    RName& GetName() { return name; }
    size_t GetIndex() { return index; }

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
