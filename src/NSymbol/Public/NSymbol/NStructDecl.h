#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <memory>
#include <optional>
#include <span>

#include "RSymbol/RStructDecl.h"

#include "NStructCtorDecl.h"
#include "NStructFuncDecl.h"
#include "NStructVarDecl.h"
#include "NTypeDeclContainerComponent.h"
#include "NFuncDeclContainerComponent.h"

namespace Citron {

class RType_Struct;
class NStructDtorDecl;
class NTypeParamDecl;

using RFactoryPtr = std::shared_ptr<class RFactory>;

class NStructDecl
    : public RStructDecl
{
    

public:
    NSYMBOL_API NStructDecl(NTypeDeclOuter* outer, RAccessor accessor, RName&& name, const RFactoryPtr& rFactory);
    using NGenericsComponent::InitTypeParams;
    NSYMBOL_API void InitBaseTypes(RType_Struct* baseStruct, std::vector<RType*>&& interfaces);

public:
    using NTypeDeclContainerComponent::AddType;
    NSYMBOL_API void AddCtor(NStructCtorDecl* decl);
    NSYMBOL_API void AddDtor(NStructDtorDecl* decl);
    NSYMBOL_API void AddFunc(NStructFuncDecl* decl) { NFuncDeclContainerComponent<NStructFuncDecl>::AddFunc(decl); }
    NSYMBOL_API void AddVar(NStructVarDecl* decl);

    NSYMBOL_API std::span<NStructCtorDecl*> GetCtors() { return ctors; }
    NSYMBOL_API std::span<NStructVarDecl*> GetVars() { return vars; }
    NSYMBOL_API NStructCtorDecl* GetUnboundTrivialCtor_NStructCtorDecl();

    NSYMBOL_API size_t GetVarCount() { return vars.size(); }
    NSYMBOL_API NStructVarDecl* GetUnboundVar(size_t index) { return vars[index]; }

    NTypeDeclOuter* GetNTypeDeclOuter() { return outer; }

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RTypeDecl* GetRTypeDecl() override { return this; }
    RDeclRes ToRDeclRes(RTypeArguments* typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NTypeDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }
    RType* GetOpenType() override;

    // from RTypeDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RStructDecl
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    AnyPtrSizedRange<RTypeParamDecl*> GetTypeParams() override { return NGenericsComponent::GetTypeParams(); }
    NSYMBOL_API RType_Struct* GetUnboundBaseStruct() override;
    NSYMBOL_API AnyPtrSizedRange<RStructVarDecl*> GetRVars() override;
    NSYMBOL_API std::optional<RDeclRes_StructVar> GetVar(RTypeArguments* typeArgs, const RName& name) override;
    NSYMBOL_API std::vector<RStructCtorDecl*> GetUnboundCtors() override;
    NSYMBOL_API RStructCtorDecl* GetUnboundCopyCtor() override;
    NSYMBOL_API RStructCtorDecl* GetUnboundTrivialCtor_RStructCtorDecl() override { return GetUnboundTrivialCtor_NStructCtorDecl(); }

};

} // namespace Citron