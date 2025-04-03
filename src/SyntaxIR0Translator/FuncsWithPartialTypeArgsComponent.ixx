export module Citron.SyntaxIR0Translator:FuncsWithPartialTypeArgsComponent;

import <memory>;
import <vector>;

import Citron.RDecls;

namespace Citron::SyntaxIR0Translator {

export template<typename TFuncDecl>
class FuncsWithPartialTypeArgsComponent
{
public:
    std::vector<DeclWithOuterTypeArgs<TFuncDecl>> items;
    std::shared_ptr<RTypeArguments> partialTypeArgsExceptOuter; // outer부분을 제외한 typeArgs면서 완전하지 않을수도 있는 typeArgs

public:
    FuncsWithPartialTypeArgsComponent(const std::vector<DeclWithOuterTypeArgs<TFuncDecl>> items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter)
        : items(items), partialTypeArgsExceptOuter(partialTypeArgsExceptOuter)
    {
    }

    size_t GetCount() { return items.size(); }
    const std::shared_ptr<TFuncDecl>& GetDecl(size_t i) { return items[i].decl; }
    RTypeArgumentsPtr GetOuterTypeArgs(int i) { return items[i].outerTypeArgs; }
    RTypeArgumentsPtr GetPartialTypeArgsExceptOuter() { return partialTypeArgsExceptOuter; }
};

} // namespace Citron::SyntaxIR0Translator