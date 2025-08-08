#pragma once

#include "IR0Config.h"

#include "RClassFuncDecl.h"

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "NCommonFuncDeclComponent.h"

namespace Citron {

class NClassFuncDecl
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RClassFuncDecl
    , private NCommonFuncDeclComponent
{
public:
    using RDeclType = RClassFuncDecl;
    using RMemberType = RMember_ClassFuncs;

public:
    NClassDecl* _class;
    RAccessor accessor;
    RName name;
    std::vector<std::string> typeParams;
    bool bStatic;

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDecl
    NDecl* GetNDecl() override { return this; }
    RFuncReturn GetUnboundFuncReturn() override { return NCommonFuncDeclComponent::GetUnboundFuncReturn(); }
    bool IsSeqFunc() override { return NCommonFuncDeclComponent::IsSeqFunc(); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    // from RFuncDecl
    size_t GetTypeParamCount() override { return NCommonFuncDeclComponent::GetTypeParamCount(); }

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RClassFuncDecl
    RType* GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }
};

}