#pragma once

#include "MNamespaceDeclContainerComponent.h"
#include "MTypeDeclContainerComponent.h"
#include "MFuncDeclContainerComponent.h"
#include "MGlobalFuncDecl.h"

namespace Citron {

class MModuleDecl 
    : private MNamespaceDeclContainerComponent
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MGlobalFuncDecl>>

{
    std::string moduleName;
    bool bReference; // 지금 만들고 있는 모듈인지, 아닌지 여부를 판별할때 쓴다
};

}