#pragma once

#include "NDecl.h"
#include "NTypeDecl.h"
#include "NTypeDeclOuter.h"
#include "NClassConstructorDecl.h"
#include "NClassMemberFuncDecl.h"
#include "NClassMemberVarDecl.h"
#include "RNames.h"
#include "NTypeDeclContainerComponent.h"
#include "NFuncDeclContainerComponent.h"
#include "NTypeDeclOuter.h"
#include "RAccessor.h"
#include "RClassDecl.h"

namespace Citron
{

class RType_Class;
class RType_Interface;

class NClassDecl
    : public NDecl
    , public NTypeDecl
    , public NTypeDeclOuter
    , public NFuncDeclOuter
    , public RClassDecl
    , private NTypeDeclContainerComponent
    , private NFuncDeclContainerComponent<NClassMemberFuncDecl>
{
    struct BaseTypes
    {
        std::shared_ptr<RType_Class> baseClass;
        std::vector<RType_Interface> interfaces;
    };

    NTypeDeclOuterWPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;

    std::vector<std::shared_ptr<NClassConstructorDecl>> constructors;
    int trivialConstructorIndex; // can be -1

    std::vector<std::shared_ptr<NClassMemberVarDecl>> memberVars;

    std::optional<BaseTypes> oBaseTypes;

public:    
    // from NDecl
    RAccessor GetAccessor() override { return accessor; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from NTypeDeclOuter, NFuncDeclOuter, NDecl
    IR0_API NDecl* GetDecl() override;

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this);  }
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NTypeDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}