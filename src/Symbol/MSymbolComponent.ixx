export module Citron.Symbols:MSymbolComponent;

import <memory>;
import <vector>;
import :MType;

namespace Citron
{

template<typename TOuter, typename TDecl>
class TMSymbolComponent
{
    TOuter outer;
    std::shared_ptr<TDecl> decl;
    std::vector<MType> typeArgs;
};

}