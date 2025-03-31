export module Citron.RDecls:DeclWithOuterTypeArgs;

import <memory>;

namespace Citron {

export class RTypeArguments;
export using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

export template<typename TDecl>
struct DeclWithOuterTypeArgs
{
    std::shared_ptr<TDecl> decl;
    RTypeArgumentsPtr outerTypeArgs;
};

} // namespace Citron