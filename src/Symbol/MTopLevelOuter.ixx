export module Citron.Symbols:MTopLevelOuter;

import <variant>;
import <memory>;

class MModuleDecl;
class MNamespaceDecl;

namespace Citron
{

export using MTopLevelOuter = std::variant<
    std::weak_ptr<MModuleDecl>,
    std::weak_ptr<MNamespaceDecl>
>;

}