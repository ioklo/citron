#pragma once
#include "IR0Config.h"

#include <memory>
#include <vector>

#include "RDecl.h"
#include "RFuncDeclOuter.h"
#include "RFuncDecl.h"
#include "RAccessor.h"
#include "RCommonFuncDeclComponent.h"

namespace Citron
{

class RStructDecl;
struct RFuncParameter;

class RStructConstructorDecl 
    : public RFuncDecl
    , public RFuncDeclOuter
    , private RCommonFuncDeclComponent
{
public:
    std::weak_ptr<RStructDecl> _struct;
    RAccessor accessor;
    bool bTrivial;

public:
    IR0_API RStructConstructorDecl(std::weak_ptr<RStructDecl> _struct, RAccessor accessor, bool bTrivial);
    IR0_API void InitFuncParameters(std::vector<RFuncParameter> parameters, bool bLastParameterVariadic);
    using RCommonFuncDeclComponent::InitBody;
    IR0_API ~RStructConstructorDecl();

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