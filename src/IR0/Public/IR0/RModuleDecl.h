#pragma once

#include "RDecl.h"
#include "RTopLevelOuter.h"
#include "RTopLevelDeclOuter.h"
#include "RTypeDeclOuter.h"
#include "RTypeOuter.h"
#include "RNamespaceDeclContainerComponent.h"
#include "RTypeDeclContainerComponent.h"
#include "RFuncDeclContainerComponent.h"
#include "RGlobalFuncDecl.h"

namespace Citron {

class RModuleDecl 
    : public RDecl
    , public RTopLevelOuter
    , public RTopLevelDeclOuter
    , public RTypeDeclOuter
    , public RTypeOuter
    , private RNamespaceDeclContainerComponent
    , private RTypeDeclContainerComponent
    , private RFuncDeclContainerComponent<std::shared_ptr<RGlobalFuncDecl>>

{
    std::string moduleName;
    bool bReference; // 지금 만들고 있는 모듈인지, 아닌지 여부를 판별할때 쓴다

public:
    using RNamespaceDeclContainerComponent::AddNamespace;
    using RNamespaceDeclContainerComponent::GetNamespace;
    using RTypeDeclContainerComponent::AddType;

    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTopLevelOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTopLevelDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}