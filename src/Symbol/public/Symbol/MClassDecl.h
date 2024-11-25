#pragma once

#include "MDecl.h"
#include "MTypeDecl.h"
#include "MTypeDeclOuter.h"
#include "MClassCtorDecl.h"
#include "MClassFuncDecl.h"
#include "MClassVarDecl.h"
#include "MNames.h"
#include "MTypeDeclContainerComponent.h"
#include "MFuncDeclContainerComponent.h"
#include "MTypeDeclOuter.h"
#include "MAccessor.h"

namespace Citron
{

class MClassDecl
    : public MDecl
    , public MTypeDecl
    , public MTypeDeclOuter
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MClassFuncDecl>>
{
    struct BaseTypes
    {   
        MTypePtr baseClass;
        std::vector<MTypePtr> interfaces;
    };

    MTypeDeclOuterWPtr outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<MClassCtorDecl>> ctors;
    int trivialCtorIndex; // can be -1

    std::vector<std::shared_ptr<MClassVarDecl>> vars;

    std::optional<BaseTypes> oBaseTypes;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this);  }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}