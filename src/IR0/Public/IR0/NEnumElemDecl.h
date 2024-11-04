#pragma once
#include "IR0Config.h"

#include <optional>
#include <vector>
#include <memory>
#include <unordered_map>

#include "NDecl.h"
#include "NTypeDecl.h"
#include "NEnumElemMemberVarDecl.h"

#include "REnumElemDecl.h"
#include "RMember.h"

namespace Citron
{

class NEnumDecl;

class NEnumElemDecl
    : public NDecl
    , public NTypeDecl
    , public REnumElemDecl
{
public:
    std::weak_ptr<NEnumDecl> _enum;
    std::string name;
    std::vector<std::shared_ptr<NEnumElemMemberVarDecl>> memberVars; // lazy
    std::unordered_map<std::string, std::shared_ptr<NEnumElemMemberVarDecl>> memberVarsMap;

public:
    IR0_API NEnumElemDecl(std::weak_ptr<NEnumDecl> _enum, std::string name, size_t memberVarCount);
    IR0_API void AddMemberVar(const std::shared_ptr<NEnumElemMemberVarDecl>& memberVar);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RMember ToRMember(const std::shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    
    // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }

    // from REnumElemDecl    
    IR0_API std::optional<RMember_EnumElemMemberVar> GetMemberVar(const RTypeArgumentsPtr& typeArgs, const RName& name) override;
    IR0_API size_t GetMemberVarCount() override;
    bool IsStandalone() override { return memberVars.empty(); }
    IR0_API std::vector<RFuncParameter> GetUnboundConstructorParams() override;
};

}