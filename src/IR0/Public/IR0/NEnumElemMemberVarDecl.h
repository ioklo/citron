#pragma once
#include "IR0Config.h"

#include <memory>
#include <optional>

#include "NDecl.h"
#include "RNames.h"
#include "RType.h"
#include "REnumElemMemberVarDecl.h"
#include "RMember.h"

namespace Citron
{

class NEnumElemDecl;

class NEnumElemMemberVarDecl
    : public NDecl
    , public REnumElemMemberVarDecl
{   
public:
    std::weak_ptr<NEnumElemDecl> enumElem;
    std::string name;

    RTypePtr declType; // lazy-init

public:
    IR0_API NEnumElemMemberVarDecl(std::weak_ptr<NEnumElemDecl> outer, const std::string& name);
    IR0_API void InitDeclType(RTypePtr&& declType);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    // from REnumElemMemberVarDecl
    IR0_API RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};


}