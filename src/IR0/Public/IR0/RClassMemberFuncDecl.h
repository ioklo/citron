#pragma once

#include "RDecl.h"
#include "RFuncDeclOuter.h"
#include "RFuncDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "RCommonFuncDeclComponent.h"

namespace Citron
{

class RClassDecl;

class RClassMemberFuncDecl 
    : public RFuncDecl
    , public RFuncDeclOuter
    , private RCommonFuncDeclComponent
{
public:
    std::weak_ptr<RClassDecl> _class;
    RAccessor accessor;
    RName name;
    std::vector<std::string> typeParams;
    bool bStatic;

public:
    // from RDecl
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API RDecl* GetOuter() override;

    // from RFuncDeclOuter
    IR0_API RDecl* GetDecl() override;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) override { visitor.Visit(*this); }

    using RCommonFuncDeclComponent::GetReturnType;
};

}