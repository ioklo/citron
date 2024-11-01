#pragma once

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "NCommonFuncDeclComponent.h"

#include "RClassMemberFuncDecl.h"

namespace Citron
{

class NClassDecl;

class NClassMemberFuncDecl
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RClassMemberFuncDecl
    , private NCommonFuncDeclComponent
{
public:
    using RDeclType = RClassMemberFuncDecl;
    using RMemberType = RMember_ClassMemberFuncs;

public:
    std::weak_ptr<NClassDecl> _class;
    RAccessor accessor;
    RName name;
    std::vector<std::string> typeParams;
    bool bStatic;

public:
    // from NDecl
    RAccessor GetAccessor() override { return accessor; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API NDecl* GetOuter() override;

    // from NFuncDeclOuter
    IR0_API NDecl* GetDecl() override;

    // from RClassMemberFuncDecl
    RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }


    using NCommonFuncDeclComponent::GetTypeParamCount;


    // from RDecl
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
    
};

}