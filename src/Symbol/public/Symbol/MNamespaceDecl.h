#pragma once
#include "SymbolConfig.h"

#include <string>

#include "MDecl.h"
#include "MTopLevelDeclOuter.h"
#include "MTypeDeclOuter.h"
#include "MNamespaceDeclContainerComponent.h"
#include "MTypeDeclContainerComponent.h"
#include "MFuncDeclContainerComponent.h"
#include "MGlobalFuncDecl.h"

namespace Citron
{

class MNamespaceDecl 
    : public MDecl
    , public MTopLevelDeclOuter
    , public MTypeDeclOuter
    , private MNamespaceDeclContainerComponent
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MGlobalFuncDecl>>
{
    MTopLevelDeclOuterPtr outer;
    std::string name;

public:
    SYMBOL_API MNamespaceDecl(MTopLevelDeclOuterPtr outer, std::string name);

    const std::string& GetName() { return name; }

    using MNamespaceDeclContainerComponent::AddNamespace;
    using MNamespaceDeclContainerComponent::GetNamespace;

    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTopLevelDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}
