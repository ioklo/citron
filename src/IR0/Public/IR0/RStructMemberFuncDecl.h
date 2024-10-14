#pragma once

#include <memory>
#include <vector>

#include "RDecl.h"
#include "RFuncDeclOuter.h"
#include "RFuncDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "RCommonFuncDeclComponent.h"

namespace Citron
{
class RStructDecl;

class RStructMemberFuncDecl 
    : public RFuncDecl
    , public RFuncDeclOuter
    , private RCommonFuncDeclComponent
{
public:
    std::weak_ptr<RStructDecl> _struct;
    RAccessor accessor;
    std::string name;
    std::vector<std::string> typeParams;
    bool bStatic;

public:
    IR0_API RStructMemberFuncDecl(std::weak_ptr<RStructDecl> _struct, RAccessor accessor, std::string name, std::vector<std::string> typeParams, bool bStatic);
    IR0_API void InitFuncReturnAndParams(RTypePtr funcReturn, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic);
    using RCommonFuncDeclComponent::InitBody;

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

    using RCommonFuncDeclComponent::GetReturnType;
};

}