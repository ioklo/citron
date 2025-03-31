export module Citron.NDecls:NModule;

import <string>;
import <memory>;

import Citron.RDecls;

namespace Citron {

export class NNamespaceDecl;

export class NModule : public RModule
{
public:
    std::string name;
    std::shared_ptr<NNamespaceDecl> rootNamespace;

public:
    NModule(std::string&& name, std::shared_ptr<NNamespaceDecl>&& rootNamespace);
};

}