#pragma once
#include <vector>
#include <memory>
#include <string>

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "RAccessor.h"
#include "NCommonFuncDeclComponent.h"

#include "RClassConstructorDecl.h"

namespace Citron
{

class NClassDecl;
struct RFuncParameter;

class NClassConstructorDecl 
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RClassConstructorDecl
    , private NCommonFuncDeclComponent
{
public:
    std::weak_ptr<NClassDecl> _class;
    RAccessor accessor;
    bool bTrivial;

public:
    NClassConstructorDecl(const std::shared_ptr<NClassDecl>& _class, RAccessor accessor, bool bTrivial, std::vector<std::string>&& typeParams, std::vector<RFuncParameter> parameters, bool bLastParamVariadic);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDecl
    NDecl* GetNDecl() override { return this; }
    RFuncReturn GetOpenFuncReturn() override { return RFuncReturn_ForConstructor(); }
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

    // from RClassConstructorDecl
    IR0_API std::shared_ptr<RClassDecl> GetClassDecl() override;
};


}