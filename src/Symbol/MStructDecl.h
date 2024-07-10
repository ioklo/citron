#pragma once

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
    : private MTypeDeclContainerComponent
    , private MFuncDeclContainerComponent<std::shared_ptr<MStructMemberFuncDecl>>
{
    struct BaseTypes
    {
        std::shared_ptr<MStruct> baseStruct;
        std::vector<std::shared_ptr<MInterface>> interfaces;
    };

    MTypeDeclOuter outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<MStructConstructorDecl>> constructors;
    int trivialConstructorIndex; // can be -1

    std::vector<std::shared_ptr<MStructMemberVarDecl>> memberVars;

    std::optional<BaseTypes> oBaseTypes;
};

}