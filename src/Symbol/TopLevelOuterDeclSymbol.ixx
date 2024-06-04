export module Citron.DeclSymbols:TopLevelOuterDeclSymbol;

// circular dependency 때문에, import 하지는 못한다
// import :ModuleDeclSymbol;

import <variant>;
import <memory>;

namespace Citron {

class ModuleDeclSymbol;
class NamespaceDeclSymbol;

export using TopLevelOuterDeclSymbol = std::variant<
    std::weak_ptr<ModuleDeclSymbol>,
    std::weak_ptr<NamespaceDeclSymbol>
>;

}