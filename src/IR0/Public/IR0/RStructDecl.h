#pragma once

#include <vector>
#include <optional>
#include <memory>

#include "RDecl.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"
#include "RNames.h"
#include "RStructConstructorDecl.h"
#include "RStructMemberFuncDecl.h"
#include "RStructMemberVarDecl.h"
#include "RTypeDeclContainerComponent.h"
#include "RFuncDeclContainerComponent.h"
#include "RTypeDeclOuter.h"
#include "RAccessor.h"

namespace Citron
{

class RStruct;
class RInterface;

class RStructDecl 
    : public RDecl
    , public RTypeDecl
    , public RTypeDeclOuter
    , private RTypeDeclContainerComponent
    , private RFuncDeclContainerComponent<std::shared_ptr<RStructMemberFuncDecl>>
{
    struct BaseTypes
    {
        std::shared_ptr<RStruct> baseStruct;
        std::vector<std::shared_ptr<RInterface>> interfaces;
    };

    RTypeDeclOuterPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<RStructConstructorDecl>> constructors;
    int trivialConstructorIndex; // can be -1

    std::vector<std::shared_ptr<RStructMemberVarDecl>> memberVars;

    std::optional<BaseTypes> oBaseTypes;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}