export module Citron.Symbols:MClass;

import <vector>;
import :MClassDecl;
import :MSymbolComponent;

namespace Citron
{

using MClassOuter = std::variant<
    std::weak_ptr<class MModuleDecl>,
    std::weak_ptr<class MNamespaceDecl>,
    std::weak_ptr<class MClass>,
    std::weak_ptr<class MStruct>
>;

class MClass : private TMSymbolComponent<MClassOuter, MClassDecl>
{   
};

}