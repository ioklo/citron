#pragma once

#include "MDecl.h"
#include "MTopLevelOuter.h"
#include "MTopLevelDeclOuter.h"
#include "MTypeDeclOuter.h"
#include "MTypeOuter.h"
#include "MNamespaceDeclContainerComponent.h"
#include "MTypeDeclContainerComponent.h"
#include "MFuncDeclContainerComponent.h"
#include "MGlobalFuncDecl.h"

namespace Citron {

class MModuleDecl 
    : public MDecl
    , public MTopLevelOuter
    , public MTopLevelDeclOuter
    , public MTypeDeclOuter
    , public MTypeOuter
    , private MNamespaceDeclContainerComponent
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MGlobalFuncDecl>>

{
    std::string moduleName;
    bool bReference; // 지금 만들고 있는 모듈인지, 아닌지 여부를 판별할때 쓴다

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTopLevelOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTopLevelDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeOuterVisitor& visitor) override { visitor.Visit(*this); }
};

using MModuleDeclPtr = std::shared_ptr<MModuleDecl>;

}