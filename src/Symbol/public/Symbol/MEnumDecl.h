#pragma once

#include <vector>
#include <optional>
#include <memory>

#include "MDecl.h"
#include "MTypeDecl.h"
#include "MAccessor.h"
#include "MNames.h"
#include "MEnumElemDecl.h"
#include "MTypeDeclOuter.h"

namespace Citron
{

class MEnumDecl
    : public MDecl
    , public MTypeDecl
{
    MTypeDeclOuterPtr outer;
    MAccessor accessor;

    MName name;
    std::vector<std::string> typeParams;

    std::optional<std::vector<std::shared_ptr<MEnumElemDecl>>> elems; // lazy initialization

    // std::unordered_map<std::string, int> elemsByName;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}

