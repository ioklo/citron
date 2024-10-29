#pragma once

#include "IR0Config.h"

#include <memory>

#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

namespace Citron {

class MGlobalFuncDecl;
class NDecl;
class RIdentifier;
class RTypeArguments;
class RTypeFactory;

using RTypePtr = std::shared_ptr<class RType>;

// abstract
class RGlobalFuncDecl
    : public RFuncDecl
    , public RFuncDeclOuter
{
public:
    // from RFuncDecl
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from NFuncDeclOuter
    IR0_API NDecl* GetDecl() override;

public:
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) override { visitor.Visit(*this); }

    virtual RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
};

class RMGlobalFuncDecl : public RGlobalFuncDecl
{
    std::shared_ptr<MGlobalFuncDecl> externalFuncDecl;
};



}