#pragma once

#include "MTopLevelOuter.h"
#include "MNamespaceDeclContainerComponent.h"
#include "MTypeDeclContainerComponent.h"
#include "MFuncDeclContainerComponent.h"
#include "MGlobalFuncDecl.h"

namespace Citron
{

class MNamespaceDecl 
    : private MNamespaceDeclContainerComponent
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MGlobalFuncDecl>>
{
    MTopLevelOuter outer;
    std::string name;

    MNamespaceDeclContainerComponent topLevelComp;
};

}
