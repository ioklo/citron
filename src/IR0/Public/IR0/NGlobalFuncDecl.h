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
#include "NCommonFuncDeclComponent.h"
#include "RGlobalFuncDecl.h"

namespace Citron {

class MGlobalFuncDecl;
class NNamespaceDecl;

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
    std::weak_ptr<NNamespaceDecl> outer;
    RAccessor accessor;    
    RName name;

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDecl
    NDecl* GetNDecl() override { return this; }
    using NCommonFuncDeclComponent::GetOpenFuncReturn;
    using NCommonFuncDeclComponent::IsSeqFunc;
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    // from RFuncDecl
    using NCommonFuncDeclComponent::GetTypeParamCount;

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }
};

}