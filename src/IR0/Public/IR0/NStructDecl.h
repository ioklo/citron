#pragma once

#include "IR0Config.h"

#include <vector>
#include <optional>
#include <ranges>

#include "RStructDecl.h"

#include "NDecl.h"
#include "NTypeDecl.h"
#include "NTypeDeclOuter.h"
#include "NStructCtorDecl.h"
#include "NStructFuncDecl.h"
#include "NStructVarDecl.h"
#include "NTypeDeclContainerComponent.h"
#include "NFuncDeclContainerComponent.h"
#include "NTypeDeclOuter.h"

namespace Citron {

class RType_Struct;
class RType_Interface;

class NStructDecl
    : public NDecl
    , public NTypeDecl
    , public NTypeDeclOuter
    , public NFuncDeclOuter
    , public RStructDecl
    , private NTypeDeclContainerComponent
    , private NFuncDeclContainerComponent<NStructFuncDecl>
{
    struct BaseTypes
    {
        RType_Struct* baseStruct;
        std::vector<RType_Interface*> interfaces;
    };

    NTypeDeclOuter* outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;

    std::vector<NStructCtorDecl*> ctors;
    int trivialCtorIndex; // can be -1

    std::vector<NStructVarDecl*> vars;
    std::optional<BaseTypes> oBaseTypes;

    std::unordered_map<RName, NStructVarDecl*> varsMap;

public:
    IR0_API NStructDecl(NTypeDeclOuter* outer, RAccessor accessor, RName&& name, std::vector<std::string>&& typeParams);
    IR0_API void InitBaseTypes(RType_Struct* baseStruct, std::vector<RType_Interface*>&& interfaces);

public:
    using NTypeDeclContainerComponent::AddType;
    IR0_API void AddCtor(NStructCtorDecl* decl);
    IR0_API void AddFunc(NStructFuncDecl* decl) { NFuncDeclContainerComponent<NStructFuncDecl>::AddFunc(decl); }
    IR0_API void AddVar(NStructVarDecl* decl);

    IR0_API auto EnumerateUnboundCtors() { return std::views::all(ctors); }
    IR0_API auto EnumerateUnboundVars() { return std::views::all(vars); }
    IR0_API NStructCtorDecl* GetUnboundTrivialCtor_NStructCtorDecl();

    IR0_API size_t GetVarCount() { return vars.size(); }
    IR0_API NStructVarDecl* GetUnboundVar(size_t index) { return vars[index]; }

    IR0_API RType_Struct* GetUnboundBaseStruct();

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RMember ToRMember(RTypeArguments* typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NTypeDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, IR0Factory& factory) override;

    // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }

    // from RTypeDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RStructDecl
    IR0_API std::optional<RMember_StructVar> GetVar(RTypeArguments* typeArgs, const RName& name) override;
    IR0_API std::vector<RStructCtorDecl*> GetUnboundCtors() override;
    IR0_API RStructCtorDecl* GetUnboundTrivialCtor_RStructCtorDecl() override { return GetUnboundTrivialCtor_NStructCtorDecl(); }

};

} // namespace Citron