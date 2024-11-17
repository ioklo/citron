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
    using RDeclType = RStructMemberFuncDecl;
    using RMemberType = RMember_StructMemberFuncs;

public:
    std::weak_ptr<NStructDecl> _struct;
    RAccessor accessor;
    std::string name;
    std::vector<std::string> typeParams;
    bool bStatic;

public:
    IR0_API NStructMemberFuncDecl(std::weak_ptr<NStructDecl> _struct, RAccessor accessor, std::string name, std::vector<std::string>&& typeParams, bool bStatic);
    IR0_API void InitFuncReturnAndParams(RTypePtr funcReturn, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic);
    using NCommonFuncDeclComponent::InitBody;

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

    // from RStructMemberFuncDecl
    RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }
};

}