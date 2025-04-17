export module Citron.NDecls:NModule;

import "IR0Config.h";
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
    IR0_API NModule(std::string&& name, std::shared_ptr<NNamespaceDecl>&& rootNamespace);
};

}