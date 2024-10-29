#pragma once
#include "IR0Config.h"

#include <memory>
#include <vector>

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "RAccessor.h"
#include "NCommonFuncDeclComponent.h"

namespace Citron
{

class NStructDecl;
struct RFuncParameter;

class NStructConstructorDecl 
    : public NFuncDecl
    , public NFuncDeclOuter
    , private NCommonFuncDeclComponent
{
public:
    std::weak_ptr<NStructDecl> _struct;
    RAccessor accessor;
    bool bTrivial;

public:
    IR0_API NStructConstructorDecl(std::weak_ptr<NStructDecl> _struct, RAccessor accessor, bool bTrivial);
    IR0_API void InitFuncParameters(std::vector<RFuncParameter> parameters, bool bLastParameterVariadic);
    using NCommonFuncDeclComponent::InitBody;
    IR0_API ~NStructConstructorDecl();

public:
    // from NDecl
    RAccessor GetAccessor() override { return accessor; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from NFuncDeclOuter
    IR0_API NDecl* GetDecl() override;

public:
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}