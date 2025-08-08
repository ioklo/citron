#pragma once

#include <vector>
#include <optional>
#include <string>

#include "MDecl.h"
#include "MTypeDecl.h"
#include "MTypeDeclOuter.h"

#include "MAccessor.h"
#include "MNames.h"

namespace Citron
{

class MEnumDecl
    : public MDecl
    , public MTypeDecl
{
    MTypeDeclOuter* outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::optional<std::vector<MEnumElemDecl*>> elems; // lazy initialization

    // std::unordered_map<std::string, int> elemsByName;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}

