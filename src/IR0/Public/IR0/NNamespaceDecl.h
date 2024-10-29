#pragma once
#include "IR0Config.h"

#include <string>

#include "NDecl.h"
#include "NTopLevelDeclOuter.h"
#include "NTypeDeclOuter.h"
#include "NNamespaceDeclContainerComponent.h"
#include "NTypeDeclContainerComponent.h"
#include "NFuncDeclContainerComponent.h"
#include "NGlobalFuncDecl.h"

#include "RNamespaceDecl.h"

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
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API NDecl* GetOuter() override;
    IR0_API RMemberPtr GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    
    // from RTypeDeclOuter, RTopLevelDeclOuter, RFuncDeclOuter
    IR0_API NDecl* GetDecl() override;

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NTopLevelDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}
