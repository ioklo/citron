#pragma once

#include "IR0Config.h"

#include <vector>
#include <optional>
#include <memory>
#include <ranges>

#include "NDecl.h"
#include "NTypeDecl.h"
#include "NTypeDeclOuter.h"
#include "RNames.h"
#include "NStructConstructorDecl.h"
#include "NStructMemberFuncDecl.h"
#include "NStructMemberVarDecl.h"
#include "NTypeDeclContainerComponent.h"
#include "NFuncDeclContainerComponent.h"
#include "NTypeDeclOuter.h"
#include "RAccessor.h"
#include "RStructDecl.h"

namespace Citron
{

class RType_Struct;
class RType_Interface;

class NStructDecl
    : public NDecl
    , public NTypeDecl
    , public NTypeDeclOuter
    , public NFuncDeclOuter
    , public RStructDecl
    , private NTypeDeclContainerComponent
    , private NFuncDeclContainerComponent<NStructMemberFuncDecl>
{
    struct BaseTypes
    {
        std::shared_ptr<RType_Struct> baseStruct;
        std::vector<std::shared_ptr<RType_Interface>> interfaces;
    };

    NTypeDeclOuterWPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<NStructConstructorDecl>> constructors;
    int trivialConstructorIndex; // can be -1

    std::vector<std::shared_ptr<NStructMemberVarDecl>> memberVars;
    std::optional<BaseTypes> oBaseTypes;

    std::unordered_map<RName, std::shared_ptr<NStructMemberVarDecl>> memberVarsMap;

public:
    IR0_API NStructDecl(NTypeDeclOuterWPtr&& outer, RAccessor accessor, RName&& name, std::vector<std::string>&& typeParams);
    IR0_API void InitBaseTypes(RTypePtr baseStruct, std::vector<RTypePtr> interfaces);

public:
    using NTypeDeclContainerComponent::AddType;
    IR0_API void AddConstructor(std::shared_ptr<NStructConstructorDecl> decl);
    void AddMemberFunc(std::shared_ptr<NStructMemberFuncDecl> decl) { NFuncDeclContainerComponent<NStructMemberFuncDecl>::AddFunc(std::move(decl)); }
    IR0_API void AddMemberVar(std::shared_ptr<NStructMemberVarDecl> decl);

    auto GetConstructorDecls() { return std::views::all(constructors); }
    auto GetMemberVarDecls() { return std::views::all(memberVars); }

    /*size_t GetMemberVarCount() { return memberVars.size(); }
    const std::shared_ptr<NStructMemberVarDecl>& GetMemberVar(size_t index) { return memberVars[index]; }*/

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RMember ToRMember(const std::shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NTypeDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }
    
    // from RTypeDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RStructDecl
    IR0_API std::optional<RMember_StructMemberVar> GetMemberVar(const RTypeArgumentsPtr& typeArgs, const RName& name) override;
    IR0_API std::vector<std::shared_ptr<RStructConstructorDecl>> GetUnboundConstructors() override;

};

}