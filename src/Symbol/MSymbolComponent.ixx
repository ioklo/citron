export module Citron.Symbols:MSymbolComponent;

import "std.h";

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