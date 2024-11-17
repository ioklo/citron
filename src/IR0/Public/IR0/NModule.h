#pragma once

#include <string>
#include <memory>

#include "RModule.h"

namespace Citron {

class NNamespaceDecl;

using RNamespaceDeclGroupPtr = std::shared_ptr<class RNamespaceDeclGroup>;

class NModule : public RModule
{
public:
    std::string name;
    std::shared_ptr<NNamespaceDecl> rootNamespace;

public:
    NModule(std::string&& name, std::shared_ptr<NNamespaceDecl>&& rootNamespace);
};

}