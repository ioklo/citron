#pragma once
#include "MSymbolComponent.h"
#include "MLambdaDecl.h"
#include "MBodyOuter.h"

namespace Citron
{

class MLambda : private MSymbolComponent<MBodyOuter, MLambdaDecl>
{
};

}