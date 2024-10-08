#pragma once

#include <memory>
#include <vector>

#include <IR0/RTypeArguments.h>
#include <IR0/RDeclWIthOuterTypeArgs.h>

namespace Citron {
namespace SyntaxIR0Translator {

template<typename TFuncDecl>
class FuncsWithPartialTypeArgsComponent
{
    std::vector<RDeclWithOuterTypeArgs<TFuncDecl>> items;
    std::shared_ptr<RTypeArguments> partialTypeArgsExceptOuter; // outer부분을 제외한 typeArgs면서 완전하지 않을수도 있는 typeArgs

public:
    FuncsWithPartialTypeArgsComponent(const std::vector<RDeclWithOuterTypeArgs<TFuncDecl>> items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter)
        : items(items), partialTypeArgsExceptOuter(partialTypeArgsExceptOuter)
    {
    }

    size_t GetCount() { return items.size(); }
    const std::shared_ptr<TFuncDecl>& GetDecl(size_t i) { return items[i].decl; }
    RTypeArgumentsPtr GetOuterTypeArgs(int i) { return items[i].outerTypeArgs; }
    RTypeArgumentsPtr GetPartialTypeArgsExceptOuter() { return partialTypeArgsExceptOuter; }
};

} // namespace SyntaxIR0Translator
} // namespace Citron