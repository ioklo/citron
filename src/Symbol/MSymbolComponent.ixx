export module Citron.Symbols:MSymbolComponent;

import <memory>;
import <vector>;
import :MType;

namespace Citron
{

// TOuter는 shared, weak or variant
template<typename TOuter, typename TDecl>
class MSymbolComponent
{
    TOuter outer;
    std::shared_ptr<TDecl> decl;
    std::vector<MType> typeArgs;
};

}