#pragma once
#include "MSymbolComponent.h"
#include "MLambda.h"
#include "MLambdaMemberVarDecl.h"

namespace Citron
{

class MLambdaMemberVar : private MSymbolComponent<MLambda, MLambdaMemberVarDecl>
{
};


}

