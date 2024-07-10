#pragma once
// circular dependency 때문에, import 하지는 못한다
// #include "ModuleDeclSymbol.h"

namespace Citron {

class MModuleDecl;
class MNamespaceDecl;

using MTopLevelOuterDecl = std::variant<
    std::weak_ptr<MModuleDecl>,
    std::weak_ptr<MNamespaceDecl>
>;

}