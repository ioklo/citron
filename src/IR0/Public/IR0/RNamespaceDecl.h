#pragma once
#include "IR0Config.h"

#include <string>

#include "RDecl.h"
#include "RTopLevelOuter.h"
#include "RTopLevelDeclOuter.h"
#include "RTypeDeclOuter.h"
#include "RTypeOuter.h"
#include "RNamespaceDeclContainerComponent.h"
#include "RTypeDeclContainerComponent.h"
#include "RFuncDeclContainerComponent.h"
#include "RGlobalFuncDecl.h"

namespace Citron
{

class RNamespaceDecl 
    : public RDecl
    , public RTopLevelOuter
    , public RTopLevelDeclOuter
    , public RTypeDeclOuter
    , public RTypeOuter
    , private RNamespaceDeclContainerComponent
    , private RTypeDeclContainerComponent
    , private RFuncDeclContainerComponent<RGlobalFuncDecl>
{
    RTopLevelOuterPtr outer;
    std::string name;

public:
    IR0_API RNamespaceDecl(RTopLevelDeclOuterPtr outer, std::string name);

    const std::string& GetName() { return name; }

    using RNamespaceDeclContainerComponent::AddNamespace;
    using RNamespaceDeclContainerComponent::GetNamespace;

    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTopLevelOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTopLevelDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}
