#pragma once

#include "MDecl.h"
#include "MTopLevelDeclOuter.h"
#include "MTypeDeclOuter.h"
#include "MNamespaceDeclContainerComponent.h"
#include "MTypeDeclContainerComponent.h"
#include "MFuncDeclContainerComponent.h"
#include "MGlobalFuncDecl.h"

namespace Citron {

class MModuleDecl 
    : public MDecl
    , public MTopLevelDeclOuter
    , public MTypeDeclOuter
    , private MNamespaceDeclContainerComponent
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MGlobalFuncDecl>>

{
    std::string moduleName;
    bool bReference; // 지금 만들고 있는 모듈인지, 아닌지 여부를 판별할때 쓴다

public:
    using MNamespaceDeclContainerComponent::AddNamespace;
    using MNamespaceDeclContainerComponent::GetNamespace;
    using MTypeDeclContainerComponent::AddType;

    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTopLevelDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}