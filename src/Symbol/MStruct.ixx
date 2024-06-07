export module Citron.Symbols:MStruct;

import <vector>;
import :MStructDecl;
import :MSymbolComponent;

namespace Citron
{

using MStructOuter = std::variant<
    std::weak_ptr<class MModuleDecl>,
    std::weak_ptr<class MNamespaceDecl>,
    std::weak_ptr<class MClass>,
    std::weak_ptr<class MStruct>
>;

class MStruct : private TMSymbolComponent<MStructOuter, MStructDecl>
{
};

}