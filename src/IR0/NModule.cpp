module Citron.NDecls:NModule;

import <cassert>;

import Citron.RDecls;

using namespace std;

namespace Citron {

NModule::NModule(string&& name, shared_ptr<NNamespaceDecl>&& rootNamespace)
    : name(move(name)), rootNamespace(move(rootNamespace))
{
}

}