#pragma once
#include <vector>
#include <memory>

#include "RDecl.h"
#include "RFuncDeclOuter.h"
#include "RFuncDecl.h"
#include "RAccessor.h"
#include "RCommonFuncDeclComponent.h"

namespace Citron
{

class RClassDecl;
struct RFuncParameter;

class RClassConstructorDecl 
    : public RFuncDecl
    , public RFuncDeclOuter
    , private RCommonFuncDeclComponent
{
public:
    std::weak_ptr<RClassDecl> _class;
    RAccessor accessor;
    std::vector<RFuncParameter> parameters;
    bool bTrivial;

public: 
    // from RDecl
    IR0_API RDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from RFuncDeclOuter
    IR0_API RDecl* GetDecl() override;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}