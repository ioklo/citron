export module Citron.Symbols:MTopLevelOuter;

import "std.h";

export class MModuleDecl;
export class MNamespaceDecl;

namespace Citron
{

export using MTopLevelOuter = std::variant<
    std::weak_ptr<MModuleDecl>,
    std::weak_ptr<MNamespaceDecl>
>;

}