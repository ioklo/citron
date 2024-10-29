#pragma once

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "NCommonFuncDeclComponent.h"

#include "RClassMemberFuncDecl.h"

namespace Citron
{

class NClassDecl;

class NClassMemberFuncDecl
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RClassMemberFuncDecl
    , private NCommonFuncDeclComponent
{
public:
    std::weak_ptr<NClassDecl> _class;
    RAccessor accessor;
    RName name;
    std::vector<std::string> typeParams;
    bool bStatic;

public:
    // from NDecl
    RAccessor GetAccessor() override { return accessor; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API NDecl* GetOuter() override;

    // from NFuncDeclOuter
    IR0_API NDecl* GetDecl() override;

public:
    // from RClassMemberFuncDecl
    RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
    
};

}