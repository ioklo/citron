#pragma once

#include "RDecl.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"
#include "RClassConstructorDecl.h"
#include "RClassMemberFuncDecl.h"
#include "RClassMemberVarDecl.h"
#include "RNames.h"
#include "RTypeDeclContainerComponent.h"
#include "RFuncDeclContainerComponent.h"
#include "RTypeDeclOuter.h"
#include "RAccessor.h"

namespace Citron
{

class RClass;
class RInterface;

class RClassDecl
    : public RDecl
    , public RTypeDecl
    , public RTypeDeclOuter
    , private RTypeDeclContainerComponent
    , private RFuncDeclContainerComponent<RClassMemberFuncDecl>
{
    struct BaseTypes
    {
        std::shared_ptr<RClass> baseClass;
        std::vector<std::shared_ptr<RInterface>> interfaces;
    };

    RTypeDeclOuterPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<RClassConstructorDecl>> constructors;
    int trivialConstructorIndex; // can be -1

    std::vector<std::shared_ptr<RClassMemberVarDecl>> memberVars;

    std::optional<BaseTypes> oBaseTypes;

public:
    RIdentifier GetIdentifier() override { return RIdentifier { name, (int)typeParams.size(), {} }; }

    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this);  }
    void Accept(RTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}