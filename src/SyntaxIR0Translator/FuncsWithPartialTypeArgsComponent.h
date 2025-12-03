#pragma once

#include <memory>
#include <vector>

namespace Citron {

template<typename TDecl>
struct DeclWithOuterTypeArgs;

class RTypeArguments;

template<typename TFuncDecl>
class FuncsWithPartialTypeArgsComponent
{
public:
    std::vector<DeclWithOuterTypeArgs<TFuncDecl>> items;
    RTypeArguments* partialTypeArgsExceptOuter; // outer부분을 제외한 typeArgs면서 완전하지 않을수도 있는 typeArgs

public:
    FuncsWithPartialTypeArgsComponent(const std::vector<DeclWithOuterTypeArgs<TFuncDecl>> items, RTypeArguments* partialTypeArgsExceptOuter)
        : items(items), partialTypeArgsExceptOuter(partialTypeArgsExceptOuter)
    {
    }

    size_t GetCount() { return items.size(); }
    TFuncDecl* GetDecl(size_t i) { return items[i].decl; }
    RTypeArguments* GetOuterTypeArgs(int i) { return items[i].outerTypeArgs; }
    RTypeArguments* GetPartialTypeArgsExceptOuter() { return partialTypeArgsExceptOuter; }
};

} // namespace Citron