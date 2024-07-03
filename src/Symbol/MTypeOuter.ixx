export module Citron.Symbols:MTypeOuter;

import "std.h";

namespace Citron
{

export class MModuleDecl;
export class MNamespaceDecl;
export class MClass;
export class MStruct;


using MTypeOuter = std::variant<
    std::shared_ptr<class MModuleDecl>,
    std::shared_ptr<class MNamespaceDecl>,
    std::shared_ptr<class MClass>,
    std::shared_ptr<class MStruct>
>;

}