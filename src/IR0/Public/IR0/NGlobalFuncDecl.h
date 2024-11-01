#pragma once

#include <vector>
#include <optional>

#include "NDecl.h"
#include "NFuncDecl.h"
#include "NFuncDeclOuter.h"
#include "RAccessor.h"
#include "RNames.h"
#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "NTopLevelDeclOuter.h"
#include "NCommonFuncDeclComponent.h"
#include "RGlobalFuncDecl.h"

namespace Citron {

class MGlobalFuncDecl;

class NGlobalFuncDecl
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RGlobalFuncDecl
    , private NCommonFuncDeclComponent
{   
public:
    using RDeclType = RGlobalFuncDecl;
    using RMemberType = RMember_GlobalFuncs;

public:
    NTopLevelDeclOuterWPtr outer;
    RAccessor accessor;    
    RName name;

public:
    // from NFuncDecl
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from NFuncDeclOuter
    IR0_API NDecl* GetDecl() override;

    // from RDecl
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    using NCommonFuncDeclComponent::GetTypeParamCount;

    // accestors
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
};

}