module;
#include "SymbolConfig.h"
#include <string>
#include <memory>

export module Citron.MDecls:MNamespaceDecl;

import :MDecl;
import :MTypeDeclOuter;
import :MNamespaceDeclContainerComponent;
import :MTypeDeclContainerComponent;
import :MFuncDeclContainerComponent;
import :MGlobalFuncDecl;

namespace Citron
{

export class MNamespaceDecl
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
