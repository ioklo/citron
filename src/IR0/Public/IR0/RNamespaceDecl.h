#pragma once
#include "IR0Config.h"

#include <string>

#include "RDecl.h"
#include "RTopLevelDeclOuter.h"
#include "RTypeDeclOuter.h"
#include "RNamespaceDeclContainerComponent.h"
#include "RTypeDeclContainerComponent.h"
#include "RFuncDeclContainerComponent.h"
#include "RGlobalFuncDecl.h"

namespace Citron
{

class RNamespaceDecl 
    : public RDecl
    , public RTopLevelDeclOuter
    , public RTypeDeclOuter
    , private RNamespaceDeclContainerComponent
    , private RTypeDeclContainerComponent
    , private RFuncDeclContainerComponent<RGlobalFuncDecl>
{
    RTopLevelDeclOuterPtr outer;
    std::string name;

public:
    IR0_API RNamespaceDecl(RTopLevelDeclOuterPtr outer, std::string name);

    const std::string& GetName() { return name; }

    using RNamespaceDeclContainerComponent::AddNamespace;
    using RNamespaceDeclContainerComponent::GetNamespace;

    using RTypeDeclContainerComponent::AddType;

public:
    // from RDecl
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API RDecl* GetOuter() override;
    IR0_API RMemberPtr GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    
    // from RTypeDeclOuter, RTopLevelDeclOuter
    IR0_API RDecl* GetDecl() override;

    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTopLevelDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}
