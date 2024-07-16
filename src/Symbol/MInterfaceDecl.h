#pragma once

#include <vector>
#include "MDecl.h"
#include "MTypeDecl.h"
#include "MTypeDeclOuter.h"
#include "MAccessor.h"
#include "MNames.h"

namespace Citron
{

class MInterfaceDecl
    : public MDecl
    , public MTypeDecl
{
    MTypeDeclOuterPtr outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}