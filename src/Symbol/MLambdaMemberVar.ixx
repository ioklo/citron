export module Citron.Symbols:MLambdaMemberVar;

import :MSymbolComponent;
import :MLambda;
import :MLambdaMemberVarDecl;

namespace Citron
{

export class MLambdaMemberVar : private MSymbolComponent<MLambda, MLambdaMemberVarDecl>
{
};


}

