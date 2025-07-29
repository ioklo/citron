#pragma once

#include "IR0Config.h"

#include <string>
#include <optional>
#include <memory>

#include "RNamespaceDecl.h"

#include "NDecl.h"
#include "NTypeDeclOuter.h"
#include "NNamespaceDeclContainerComponent.h"
#include "NTypeDeclContainerComponent.h"
#include "NFuncDeclContainerComponent.h"
#include "NGlobalFuncDecl.h"

namespace Citron
{

class RNamespaceDeclGroup;
using RNamespaceDeclGroupPtr = std::shared_ptr<RNamespaceDeclGroup>;

class NNamespaceDecl
    : public NDecl
    , public NTypeDeclOuter
    , public NFuncDeclOuter
    , public RNamespaceDecl
    , private NNamespaceDeclContainerComponent
    , private NTypeDeclContainerComponent
    , private NFuncDeclContainerComponent<NGlobalFuncDecl>
{
public:
    using RDeclType = RNamespaceDecl;
    using RMemberType = RMember_Namespace;

private:
    std::weak_ptr<NNamespaceDecl> outer;
    std::string name;
    RNamespaceDeclGroupPtr group;

public:
    IR0_API static std::shared_ptr<NNamespaceDecl> MakeRoot(RTypeFactory& factory);
    IR0_API static std::shared_ptr<NNamespaceDecl> MakeChild(const std::shared_ptr<NNamespaceDecl>& outer, const std::string& name, RTypeFactory& factory);

private:
    NNamespaceDecl(const std::shared_ptr<NNamespaceDecl>& outer, const std::string& name, const RNamespaceDeclGroupPtr& group);

public:
    const std::string& GetName() { return name; }

    using NNamespaceDeclContainerComponent::AddNamespace;
    using NNamespaceDeclContainerComponent::GetNamespace;

    using NTypeDeclContainerComponent::AddType;

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NTypeDeclOuter
    NDecl* GetNDecl() override { return this; }
    void Accept(NTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    // from RTypeDeclOuter
    // using RNamespaceDecl::Accept;
    // RDecl* GetRDecl() override { return this; }

    // from RFuncDeclOuter
    // using RNamespaceDecl::Accept;
    // RDecl* GetRDecl() override { return this; }
};

}
