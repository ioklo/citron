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

    // from RTypeDecl
    NDecl* GetDecl() override { return this; }

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}