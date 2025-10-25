#pragma once

#include <vector>
#include <string>


#include "EDecl.h"
#include "ETypeDecl.h"
#include "ETypeDeclOuter.h"

#include "EAccessor.h"
#include "ENames.h"

namespace Citron
{

class EInterfaceDecl
    : public EDecl
    , public ETypeDecl
{
    ETypeDeclOuter* outer;
    EAccessor accessor;

    EName name;
    std::vector<std::string> typeParams;

public:
    void Accept(EDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(ETypeDeclVisitor& visitor) override { visitor.Visit(this); }
};

}