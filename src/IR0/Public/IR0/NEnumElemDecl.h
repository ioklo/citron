#pragma once
#include "IR0Config.h"

#include <optional>
#include <vector>
#include <memory>

#include "NDecl.h"
#include "NTypeDecl.h"
#include "NEnumElemMemberVarDecl.h"

#include "REnumElemDecl.h"

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

public:
    IR0_API NEnumElemDecl(std::weak_ptr<NEnumDecl> _enum, std::string name, size_t memberVarCount);
    IR0_API void AddMemberVar(std::shared_ptr<NEnumElemMemberVarDecl> memberVar);
    bool IsStandalone() { return memberVars.empty(); }
    IR0_API std::vector<RFuncParameter> GetUnboundConstructorParams();

public:
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API RMemberPtr GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}