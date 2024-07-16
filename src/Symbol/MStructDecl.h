#pragma once

#include "MDecl.h"
#include "MTypeDecl.h"
#include "MTypeDeclOuter.h"
#include "MNames.h"
#include "MStructConstructorDecl.h"
#include "MStructMemberFuncDecl.h"
#include "MStructMemberVarDecl.h"
#include "MTypeDeclContainerComponent.h"
#include "MFuncDeclContainerComponent.h"
#include "MTypeDeclOuter.h"
#include "MAccessor.h"

namespace Citron
{

class MStruct;
class MInterface;

class MStructDecl 
    : public MDecl
    , public MTypeDecl
    , public MTypeDeclOuter
    , private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MStructMemberFuncDecl>>
{
    struct BaseTypes
    {
        std::shared_ptr<MStruct> baseStruct;
        std::vector<std::shared_ptr<MInterface>> interfaces;
    };

    MTypeDeclOuterPtr outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<MStructConstructorDecl>> constructors;
    int trivialConstructorIndex; // can be -1

    std::vector<std::shared_ptr<MStructMemberVarDecl>> memberVars;

    std::optional<BaseTypes> oBaseTypes;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}