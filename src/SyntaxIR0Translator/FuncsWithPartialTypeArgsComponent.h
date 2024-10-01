#pragma once

#include <memory>
#include <vector>

#include <IR0/RTypeArguments.h>

namespace Citron {

namespace SyntaxIR0Translator {

template<typename TFuncDecl>
class FuncsWithPartialTypeArgsComponent
{
    struct DeclWithOuterTypeArgs
    {
        std::shared_ptr<TFuncDecl> decl;
        RTypeArgumentsPtr outerTypeArgs;
    };

    std::vector<DeclWithOuterTypeArgs> data;
    std::shared_ptr<RTypeArguments> partialTypeArgs; // outer부분을 포함한 partial

public:
    size_t GetCount() { return data.size(); }
    const std::shared_ptr<TFuncDecl>& GetDecl(size_t i) { return data[i].decl; }
    RTypeArgumentsPtr GetOuterTypeArgs(int i) { return data[i].outerTypeArgs; }
    RTypeArgumentsPtr GetPartialTypeArgs() { return partialTypeArgs; }
};

} // namespace SyntaxIR0Translator

} // namespace Citron