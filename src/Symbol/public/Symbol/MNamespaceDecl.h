#pragma once
#include "SymbolConfig.h"

#include <string>
#include <memory>


#include "MDecl.h"
#include "MTypeDeclOuter.h"
#include "MNamespaceDeclContainerComponent.h"
#include "MTypeDeclContainerComponent.h"
#include "MFuncDeclContainerComponent.h"
#include "MGlobalFuncDecl.h"

namespace Citron
{

class MNamespaceDecl
    : public MDecl
    , public MTypeDeclOuter
    , private MNamespaceDeclContainerComponent
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MGlobalFuncDecl>>
{
    std::weak_ptr<MNamespaceDecl> outer;
    std::string name;

public:
    SYMBOL_API MNamespaceDecl(std::weak_ptr<MNamespaceDecl> outer, std::string name);

    const std::string& GetName() { return name; }

    using MNamespaceDeclContainerComponent::AddNamespace;
    using MNamespaceDeclContainerComponent::GetNamespace;

    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}
