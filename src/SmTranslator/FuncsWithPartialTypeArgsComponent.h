#pragma once

#include <memory>
#include <vector>

namespace Citron {

template<typename TDecl>
struct TDeclWithOuterTypeArgs;

class RTypeArguments;

template<typename TFuncDecl> requires std::derived_from<TFuncDecl, RFuncDeclBase>
class FuncsWithPartialTypeArgsComponent
{
public:
    std::vector<TDeclWithOuterTypeArgs<TFuncDecl>> items;
    RTypeArguments* memberTypeArgs; // outer부분을 제외한 typeArgs면서 완전하지 않을수도 있는 typeArgs

public:
    FuncsWithPartialTypeArgsComponent(const std::vector<TDeclWithOuterTypeArgs<TFuncDecl>> items, RTypeArguments* memberTypeArgs)
        : items{items}, memberTypeArgs{memberTypeArgs}
    {
    }

    size_t GetCount() { return items.size(); }
    TFuncDecl* GetDecl(size_t i) { return items[i].decl; }
    RTypeArguments* GetOuterTypeArgs(int i) { return items[i].outerTypeArgs; }
    RTypeArguments* GetMemberTypeArgs() { return memberTypeArgs; }
};

} // namespace Citron