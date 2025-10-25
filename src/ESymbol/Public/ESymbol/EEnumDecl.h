#pragma once

#include <vector>
#include <optional>
#include <string>

#include "EDecl.h"
#include "ETypeDecl.h"
#include "ETypeDeclOuter.h"

#include "EAccessor.h"
#include "ENames.h"

namespace Citron
{

class EEnumDecl
    : public EDecl
    , public ETypeDecl
{
    ETypeDeclOuter* outer;
    EAccessor accessor;

    EName name;
    std::vector<std::string> typeParams;

    std::optional<std::vector<EEnumElemDecl*>> elems; // lazy initialization

    // std::unordered_map<std::string, int> elemsByName;

public:
    void Accept(EDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(ETypeDeclVisitor& visitor) override { visitor.Visit(this); }
};

}

