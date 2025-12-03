#pragma once

#include <string>

#include "EDecl.h"
#include "ETypeDeclOuter.h"
#include "ENamespaceDeclContainerComponent.h"
#include "ETypeDeclContainerComponent.h"
#include "EFuncDeclContainerComponent.h"

namespace Citron {

class EModule
{
    std::string moduleName;

public:
    EModule(std::string&& moduleName);
};

}