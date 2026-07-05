#pragma once

#include "RDecl.h"

namespace Citron {

class RType;
class RFactory;

class RStructVarDecl : public RDecl
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
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) override;
    RSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron
