#pragma once
#include "IR0Config.h"

#include <optional>
#include <vector>
#include <unordered_map>

#include "REnumElemDecl.h"

#include "NDecl.h"
#include "NTypeDecl.h"
#include "NEnumElemVarDecl.h"

namespace Citron
{

class NEnumElemDecl
    : public NDecl
    , public NTypeDecl
    , public REnumElemDecl
{
public:
    NEnumDecl* _enum;
    std::string name;
    std::vector<NEnumElemVarDecl*> vars; // lazy
    std::unordered_map<std::string, NEnumElemVarDecl*> varsMap;

public:
    IR0_API NEnumElemDecl(NEnumDecl* _enum, const std::string& name, size_t varCount);
    IR0_API void AddVar(NEnumElemVarDecl* var);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RMember ToRMember(RTypeArguments* typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, IR0Factory& factory) override;

    // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }

    // from REnumElemDecl    
    IR0_API REnumDecl* GetBaseEnumDecl() override;
    IR0_API std::optional<RMember_EnumElemVar> GetVar(RTypeArguments* typeArgs, const RName& name) override;
    IR0_API size_t GetVarCount() override;
    bool IsStandalone() override { return vars.empty(); }
    IR0_API std::vector<RFuncParameter> GetUnboundCtorParams() override;
};

}