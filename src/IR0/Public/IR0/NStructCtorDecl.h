#pragma once
#include "IR0Config.h"

#include <memory>
#include <vector>

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "RAccessor.h"
#include "NCommonFuncDeclComponent.h"

#include "RStructCtorDecl.h"

namespace Citron
{

class NStructDecl;
struct RFuncParameter;

class NStructCtorDecl 
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RStructCtorDecl
    , private NCommonFuncDeclComponent
{
public:
    std::weak_ptr<NStructDecl> _struct;
    RAccessor accessor;
    bool bTrivial;

public:
    IR0_API NStructCtorDecl(std::weak_ptr<NStructDecl> _struct, RAccessor accessor, bool bTrivial);
    IR0_API void InitFuncParameters(std::vector<RFuncParameter> parameters, bool bLastParameterVariadic);
    using NCommonFuncDeclComponent::InitBody;
    IR0_API ~NStructCtorDecl();

    using NCommonFuncDeclComponent::GetUnboundFuncParam;

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDecl
    NDecl* GetNDecl() override { return this; }
    using NCommonFuncDeclComponent::GetUnboundFuncReturn;
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

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RStructCtorDecl
    IR0_API std::shared_ptr<RStructDecl> GetStructDecl() override;
};


}