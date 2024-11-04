#pragma once
#include "IR0Config.h"

#include <string>
#include <optional>

#include "NDecl.h"
#include "NTopLevelDeclOuter.h"
#include "NTypeDeclOuter.h"
#include "NNamespaceDeclContainerComponent.h"
#include "NTypeDeclContainerComponent.h"
#include "NFuncDeclContainerComponent.h"
#include "NGlobalFuncDecl.h"

#include "RNamespaceDecl.h"
#include "RMember.h"

namespace Citron
{

class NNamespaceDecl 
    : public NDecl
    , public NTopLevelDeclOuter
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
    NTopLevelDeclOuterWPtr outer;
    std::string name;

public:
    IR0_API NNamespaceDecl(NTopLevelDeclOuterWPtr outer, std::string name);

    const std::string& GetName() { return name; }

    using NNamespaceDeclContainerComponent::AddNamespace;
    using NNamespaceDeclContainerComponent::GetNamespace;

    using NTypeDeclContainerComponent::AddType;

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    
    // from NTopLevelDeclOuter
    NDecl* GetNDecl() override { return this; }
    void Accept(NTopLevelDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from NTypeDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RTopLevelDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RTypeDeclOuter
    // using RNamespaceDecl::Accept;
    // RDecl* GetRDecl() override { return this; }

    // from RFuncDeclOuter
    // using RNamespaceDecl::Accept;
    // RDecl* GetRDecl() override { return this; }
};

}
