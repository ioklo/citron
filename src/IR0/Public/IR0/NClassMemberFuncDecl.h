#pragma once

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "NCommonFuncDeclComponent.h"

namespace Citron
{

class NClassDecl;

class NClassMemberFuncDecl
    : public NFuncDecl
    , public NFuncDeclOuter
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
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }

    using NCommonFuncDeclComponent::GetReturnType;
};

}