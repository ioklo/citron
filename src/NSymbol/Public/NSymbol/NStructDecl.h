#pragma once

#include "NSymbolConfig.h"

#include <vector>
#include <memory>
#include <optional>
#include <ranges>

#include "RSymbol/RStructDecl.h"

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
class NStructDtorDecl;

using RFactoryPtr = std::shared_ptr<class RFactory>;

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
    RFactoryPtr rFactory;

    std::vector<NStructCtorDecl*> ctors;
    NStructDtorDecl* dtor;
    int trivialCtorIndex; // can be -1

    std::vector<NStructVarDecl*> vars;
    std::optional<BaseTypes> oBaseTypes;
    std::unordered_map<RName, NStructVarDecl*> varsMap;

public:
    NSYMBOL_API NStructDecl(NTypeDeclOuter* outer, RAccessor accessor, RName&& name, std::vector<std::string>&& typeParams, const RFactoryPtr& rFactory);
    NSYMBOL_API void InitBaseTypes(RType_Struct* baseStruct, std::vector<RType_Interface*>&& interfaces);

public:
    using NTypeDeclContainerComponent::AddType;
    NSYMBOL_API void AddCtor(NStructCtorDecl* decl);
    NSYMBOL_API void AddDtor(NStructDtorDecl* decl);
    NSYMBOL_API void AddFunc(NStructFuncDecl* decl) { NFuncDeclContainerComponent<NStructFuncDecl>::AddFunc(decl); }
    NSYMBOL_API void AddVar(NStructVarDecl* decl);

    NSYMBOL_API auto EnumerateUnboundCtors() { return std::views::all(ctors); }
    NSYMBOL_API auto EnumerateUnboundVars() { return std::views::all(vars); }
    NSYMBOL_API NStructCtorDecl* GetUnboundTrivialCtor_NStructCtorDecl();

    NSYMBOL_API size_t GetVarCount() { return vars.size(); }
    NSYMBOL_API NStructVarDecl* GetUnboundVar(size_t index) { return vars[index]; }

    NSYMBOL_API RType_Struct* GetUnboundBaseStruct();
    NTypeDeclOuter* GetNTypeDeclOuter() { return outer; }

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RTypeDecl* GetRTypeDecl() override { return this; }
    RMember ToRMember(RTypeArguments* typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NTypeDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }

    // from RTypeDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RStructDecl
    NSYMBOL_API std::optional<RMember_StructVar> GetVar(RTypeArguments* typeArgs, const RName& name) override;
    NSYMBOL_API std::vector<RStructCtorDecl*> GetUnboundCtors() override;
    NSYMBOL_API RStructCtorDecl* GetUnboundTrivialCtor_RStructCtorDecl() override { return GetUnboundTrivialCtor_NStructCtorDecl(); }

};

} // namespace Citron