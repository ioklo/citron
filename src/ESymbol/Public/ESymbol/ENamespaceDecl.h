#pragma once
#include "ESymbolConfig.h"

#include <string>

#include "EDecl.h"
#include "ETypeDeclOuter.h"
#include "ENamespaceDeclContainerComponent.h"
#include "ETypeDeclContainerComponent.h"
#include "EFuncDeclContainerComponent.h"
#include "EGlobalFuncDecl.h"

namespace Citron
{

class ENamespaceDecl
    : public EDecl
    , public ETypeDeclOuter
    , private ENamespaceDeclContainerComponent
    , private ETypeDeclContainerComponent
    , private EFuncDeclContainerComponent<EGlobalFuncDecl>
{
    ENamespaceDecl* outer;
    std::string name;

public:
    ESYMBOL_API ENamespaceDecl(ENamespaceDecl* outer, std::string name);

    const std::string& GetName() { return name; }

    using ENamespaceDeclContainerComponent::AddNamespace;
    using ENamespaceDeclContainerComponent::GetNamespace;

    void Accept(EDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(ETypeDeclOuterVisitor& visitor) override { visitor.Visit(this); }
};

}
