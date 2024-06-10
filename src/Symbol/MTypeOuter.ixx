export module Citron.Symbols:MTypeOuter;

import <variant>;
import <memory>;

namespace Citron
{

using MTypeOuter = std::variant<
    std::shared_ptr<class MModuleDecl>,
    std::shared_ptr<class MNamespaceDecl>,
    std::shared_ptr<class MClass>,
    std::shared_ptr<class MStruct>
>;

}