#pragma once

#include <string>

#include "RDecl.h"
#include "RTopLevelDeclOuter.h"
#include "RTypeDeclOuter.h"
#include "RNamespaceDeclContainerComponent.h"
#include "RTypeDeclContainerComponent.h"
#include "RFuncDeclContainerComponent.h"
#include "RGlobalFuncDecl.h"

namespace Citron {

class RModuleDecl 
    : public RDecl
    , public RTopLevelDeclOuter
    , public RTypeDeclOuter
    , public RFuncDeclOuter
    , private RNamespaceDeclContainerComponent
    , private RTypeDeclContainerComponent
    , private RFuncDeclContainerComponent<RGlobalFuncDecl>

{
    std::string name;

public:
    IR0_API RModuleDecl(std::string name);

public:
    using RNamespaceDeclContainerComponent::AddNamespace;
    using RNamespaceDeclContainerComponent::GetNamespace;
    using RTypeDeclContainerComponent::AddType;

    std::string GetModuleName() override { return name; }

public:
    // from RDecl
    IR0_API RDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API RMemberPtr GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RTypeDeclOuter, RTopLevelDeclOuter, public RFuncDeclOuter
    IR0_API RDecl* GetDecl() override;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTopLevelDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}