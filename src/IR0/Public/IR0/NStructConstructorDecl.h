#pragma once
#include "IR0Config.h"

#include <memory>
#include <vector>

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "RAccessor.h"
#include "NCommonFuncDeclComponent.h"

#include "RStructConstructorDecl.h"

namespace Citron
{

class NStructDecl;
struct RFuncParameter;

class NStructConstructorDecl 
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RStructConstructorDecl
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

    // from RStructConstructorDecl
    IR0_API std::shared_ptr<RStructDecl> GetStructDecl() override;

    // from RDecl
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

public:
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}