#pragma once

#include "IR0Config.h"

#include <vector>
#include <optional>
#include <memory>
#include <ranges>

#include "RDecl.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"
#include "RNames.h"
#include "RStructConstructorDecl.h"
#include "RStructMemberFuncDecl.h"
#include "RStructMemberVarDecl.h"
#include "RTypeDeclContainerComponent.h"
#include "RFuncDeclContainerComponent.h"
#include "RTypeDeclOuter.h"
#include "RAccessor.h"

namespace Citron
{

class RStruct;
class RInterface;

class RStructDecl 
    : public RTypeDecl
    , public RTypeDeclOuter
    , private RTypeDeclContainerComponent
    , private RFuncDeclContainerComponent<RStructMemberFuncDecl>
{
    struct BaseTypes
    {
        RTypePtr baseStruct;
        std::vector<RTypePtr> interfaces;
    };

    RTypeDeclOuterPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<RStructConstructorDecl>> constructorDecls;
    int trivialConstructorIndex; // can be -1

    std::vector<std::shared_ptr<RStructMemberVarDecl>> memberVarDecls;

    std::optional<BaseTypes> oBaseTypes;

public:
    IR0_API RStructDecl(RTypeDeclOuterPtr outer, RAccessor accessor, RName name, std::vector<std::string> typeParams);
    IR0_API void InitBaseTypes(RTypePtr baseStruct, std::vector<RTypePtr> interfaces);

public:
    using RTypeDeclContainerComponent::AddType;
    IR0_API void AddConstructor(std::shared_ptr<RStructConstructorDecl> decl);
    void AddMemberFunc(std::shared_ptr<RStructMemberFuncDecl> decl) { RFuncDeclContainerComponent<RStructMemberFuncDecl>::AddFunc(std::move(decl)); }
    IR0_API void AddMemberVar(std::shared_ptr<RStructMemberVarDecl> decl);

    auto GetConstructorDecls() { return std::views::all(constructorDecls); }
    auto GetMemberVarDecls() { return std::views::all(memberVarDecls); }

    /*size_t GetMemberVarCount() { return memberVars.size(); }
    const std::shared_ptr<RStructMemberVarDecl>& GetMemberVar(size_t index) { return memberVars[index]; }*/

public:
    // from RDecl
    IR0_API RDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from RTypeDeclOuter
    IR0_API RDecl* GetDecl() override;

    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}