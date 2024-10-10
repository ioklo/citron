#pragma once
#include "IR0Config.h"

#include <optional>
#include <vector>
#include <memory>

#include "RDecl.h"
#include "RTypeDecl.h"
#include "REnumElemMemberVarDecl.h"

namespace Citron
{

class REnumDecl;

class REnumElemDecl
    : public RTypeDecl
{
public:
    std::weak_ptr<REnumDecl> _enum;
    std::string name;
    std::vector<std::shared_ptr<REnumElemMemberVarDecl>> memberVars; // lazy

public:
    IR0_API REnumElemDecl(std::weak_ptr<REnumDecl> _enum, std::string name, size_t memberVarCount);
    IR0_API void AddMemberVar(std::shared_ptr<REnumElemMemberVarDecl> memberVar);
    
    IR0_API RDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API RMemberPtr GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}