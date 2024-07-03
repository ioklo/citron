export module Citron.Symbols:MTopLevelOuterDecl;

// circular dependency 때문에, import 하지는 못한다
// import :ModuleDeclSymbol;

import "std.h";

namespace Citron {

export class MModuleDecl;
export class MNamespaceDecl;

export using MTopLevelOuterDecl = std::variant<
    std::weak_ptr<MModuleDecl>,
    std::weak_ptr<MNamespaceDecl>
>;

}