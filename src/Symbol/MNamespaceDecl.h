#pragma once

#include <string>

#include "MDecl.h"
#include "MTopLevelOuter.h"
#include "MTopLevelDeclOuter.h"
#include "MTypeDeclOuter.h"
#include "MTypeOuter.h"
#include "MNamespaceDeclContainerComponent.h"
#include "MTypeDeclContainerComponent.h"
#include "MFuncDeclContainerComponent.h"
#include "MGlobalFuncDecl.h"

namespace Citron
{

class MNamespaceDecl 
    : public MDecl
    , public MTopLevelOuter
    , public MTopLevelDeclOuter
    , public MTypeDeclOuter
    , public MTypeOuter
    , private MNamespaceDeclContainerComponent
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MGlobalFuncDecl>>
{
    std::weak_ptr<MTopLevelOuter> outer;
    std::string name;

    MNamespaceDeclContainerComponent topLevelComp;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTopLevelOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTopLevelDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}
