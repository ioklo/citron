#pragma once

#include "MNames.h"
#include "MType.h"

namespace Citron
{

class MLambdaDecl;

class MLambdaMemberVarDecl
{
    std::weak_ptr<MLambdaDecl> lambda;
    MType type;
    MName name;
};

}