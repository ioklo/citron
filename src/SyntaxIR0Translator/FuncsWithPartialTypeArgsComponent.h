#pragma once

#include <memory>
#include <vector>

#include <IR0/RTypeArguments.h>

namespace Citron {

namespace SyntaxIR0Translator {

template<typename TFuncDecl>
struct DeclWithOuterTypeArgs
{
    std::shared_ptr<TFuncDecl> decl;
    RTypeArgumentsPtr outerTypeArgs;
};

template<typename TFuncDecl>
class FuncsWithPartialTypeArgsComponent
{
    std::vector<DeclWithOuterTypeArgs<TFuncDecl>> data;
    std::shared_ptr<RTypeArguments> partialTypeArgs; // outer부분을 포함한 partial

public:
    FuncsWithPartialTypeArgsComponent(std::vector<DeclWithOuterTypeArgs<TFuncDecl>>&& data, const std::shared_ptr<RTypeArguments>& partialTypeArgs)
        : data(std::move(data)), partialTypeArgs(partialTypeArgs)
    {

    }

    size_t GetCount() { return data.size(); }
    const std::shared_ptr<TFuncDecl>& GetDecl(size_t i) { return data[i].decl; }
    RTypeArgumentsPtr GetOuterTypeArgs(int i) { return data[i].outerTypeArgs; }
    RTypeArgumentsPtr GetPartialTypeArgs() { return partialTypeArgs; }
};

} // namespace SyntaxIR0Translator

} // namespace Citron