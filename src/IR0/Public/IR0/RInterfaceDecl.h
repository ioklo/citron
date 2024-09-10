#pragma once

#include <vector>
#include "RDecl.h"
#include "RTypeDecl.h"
#include "RTypeDeclOuter.h"
#include "RAccessor.h"
#include "RNames.h"

namespace Citron
{

class RInterfaceDecl
    : public RDecl
    , public RTypeDecl
{
    RTypeDeclOuterPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;

public:
    RIdentifier GetIdentifier() override { return RIdentifier { name, (int)typeParams.size(), {} }; }

    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}