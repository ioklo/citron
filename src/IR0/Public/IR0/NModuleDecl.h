#pragma once

#include <string>

#include "NDecl.h"
#include "NTopLevelDeclOuter.h"
#include "NTypeDeclOuter.h"
#include "NNamespaceDeclContainerComponent.h"
#include "NTypeDeclContainerComponent.h"
#include "NFuncDeclContainerComponent.h"
#include "NGlobalFuncDecl.h"

namespace Citron {

class NModuleDecl 
    : public NDecl
    , public NTopLevelDeclOuter
    , public NTypeDeclOuter
    , public NFuncDeclOuter
    , private NNamespaceDeclContainerComponent
    , private NTypeDeclContainerComponent
    , private NFuncDeclContainerComponent<NGlobalFuncDecl>

{
    std::string name;

public:
    IR0_API NModuleDecl(std::string name);

public:
    using NNamespaceDeclContainerComponent::AddNamespace;
    using NNamespaceDeclContainerComponent::GetNamespace;
    using NTypeDeclContainerComponent::AddType;

    std::string GetModuleName() override { return name; }

public:
    // from NDecl
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API RMemberPtr GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RTypeDeclOuter, RTopLevelDeclOuter, public RFuncDeclOuter
    IR0_API NDecl* GetDecl() override;

public:
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NTopLevelDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}