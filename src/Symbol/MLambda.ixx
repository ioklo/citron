export module Citron.Symbols:MLambda;

import :MSymbolComponent;
import :MLambdaDecl;
import :MBodyOuter;

namespace Citron
{

export class MLambda : private MSymbolComponent<MBodyOuter, MLambdaDecl>
{
};

}