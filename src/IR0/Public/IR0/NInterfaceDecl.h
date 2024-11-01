#pragma once

#include <vector>
#include "NDecl.h"
#include "NTypeDecl.h"
#include "NTypeDeclOuter.h"
#include "RAccessor.h"
#include "RNames.h"
#include "RInterfaceDecl.h"

namespace Citron
{

class NInterfaceDecl
    : public NDecl
    , public NTypeDecl
    , public RInterfaceDecl
{
    NTypeDeclOuterWPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;

public:
    RAccessor GetAccessor() override { return accessor; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from NTypeDecl
    NDecl* GetDecl() override { return this; }
    RMember ToRMember(const std::shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs) override;

    // from RDecl
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}