export module Citron.NDecls:NStructDecl;

import "IR0Config.h";

import <vector>;
import <optional>;
import <memory>;
import <ranges>;

import Citron.RDecls;
import :NDecl;
import :NTypeDecl;
import :NTypeDeclOuter;
import :NStructCtorDecl;
import :NStructFuncDecl;
import :NStructVarDecl;
import :NTypeDeclContainerComponent;
import :NFuncDeclContainerComponent;
import :NTypeDeclOuter;

namespace Citron {

export class NStructDecl
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
        std::shared_ptr<RType_Struct> baseStruct;
        std::vector<std::shared_ptr<RType_Interface>> interfaces;
    };

    NTypeDeclOuterWPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<NStructCtorDecl>> ctors;
    int trivialCtorIndex; // can be -1

    std::vector<std::shared_ptr<NStructVarDecl>> vars;
    std::optional<BaseTypes> oBaseTypes;

    std::unordered_map<RName, std::shared_ptr<NStructVarDecl>> varsMap;

public:
    IR0_API NStructDecl(NTypeDeclOuterWPtr&& outer, RAccessor accessor, RName&& name, std::vector<std::string>&& typeParams);
    IR0_API void InitBaseTypes(std::shared_ptr<RType_Struct>&& baseStruct, std::vector<std::shared_ptr<RType_Interface>>&& interfaces);

public:
    using NTypeDeclContainerComponent::AddType;
    IR0_API void AddCtor(std::shared_ptr<NStructCtorDecl>&& decl);
    IR0_API void AddFunc(std::shared_ptr<NStructFuncDecl>&& decl) { NFuncDeclContainerComponent<NStructFuncDecl>::AddFunc(std::move(decl)); }
    IR0_API void AddVar(std::shared_ptr<NStructVarDecl>&& decl);

    IR0_API auto EnumerateUnboundCtors() { return std::views::all(ctors); }
    IR0_API auto EnumerateUnboundVars() { return std::views::all(vars); }
    IR0_API std::shared_ptr<NStructCtorDecl> GetUnboundTrivialCtor_NStructCtorDecl();

    IR0_API size_t GetVarCount() { return vars.size(); }
    IR0_API NStructVarDecl* GetUnboundVar(size_t index) { return vars[index].get(); }

    IR0_API std::shared_ptr<RType_Struct> GetUnboundBaseStruct();

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
    IR0_API std::optional<RMember_StructVar> GetVar(const RTypeArgumentsPtr& typeArgs, const RName& name) override;
    IR0_API std::vector<std::shared_ptr<RStructCtorDecl>> GetUnboundCtors() override;
    IR0_API std::shared_ptr<RStructCtorDecl> GetUnboundTrivialCtor_RStructCtorDecl() override { return GetUnboundTrivialCtor_NStructCtorDecl(); }

};

} // namespace Citron