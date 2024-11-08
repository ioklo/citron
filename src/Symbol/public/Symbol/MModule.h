#pragma once

#include "MDecl.h"
#include "MTypeDeclOuter.h"
#include "MNamespaceDeclContainerComponent.h"
#include "MTypeDeclContainerComponent.h"
#include "MFuncDeclContainerComponent.h"
#include "MGlobalFuncDecl.h"

namespace Citron {

class MModule 
{
    std::string moduleName;

public:
    MModule(std::string&& moduleName);
};

}