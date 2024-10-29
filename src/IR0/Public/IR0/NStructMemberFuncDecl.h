#pragma once

#include <memory>
#include <vector>

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "NCommonFuncDeclComponent.h"

#include "RStructMemberFuncDecl.h"

namespace Citron
{
class NStructDecl;
using RTypePtr = std::shared_ptr<class RType>;
class RTypeArguments;

class NStructMemberFuncDecl 
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RStructMemberFuncDecl
    , private NCommonFuncDeclComponent
{
public:
    std::weak_ptr<NStructDecl> _struct;
    RAccessor accessor;
    std::string name;
    std::vector<std::string> typeParams;
    bool bStatic;

public:
    IR0_API NStructMemberFuncDecl(std::weak_ptr<NStructDecl> _struct, RAccessor accessor, std::string name, std::vector<std::string> typeParams, bool bStatic);
    IR0_API void InitFuncReturnAndParams(RTypePtr funcReturn, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic);
    using NCommonFuncDeclComponent::InitBody;

public:
    // from NDecl
    RAccessor GetAccessor() override { return accessor; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from NFuncDeclOuter
    IR0_API NDecl* GetDecl() override;

    // from RStructMemberFuncDecl
    RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }
    
public:

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}