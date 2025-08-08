#pragma once

#include "IR0Config.h"

#include <memory>
#include <optional>

#include "REnumElemVarDecl.h"
#include "NDecl.h"

namespace Citron {

class NEnumElemVarDecl
    : public NDecl
    , public REnumElemVarDecl
{
public:
    NEnumElemDecl* enumElem;
    std::string name;

    RType* declType; // lazy-init

public:
    IR0_API NEnumElemVarDecl(NEnumElemDecl* outer, const std::string& name);
    IR0_API void InitDeclType(RType* declType);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    // from REnumElemVarDecl
    IR0_API RType* GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

}