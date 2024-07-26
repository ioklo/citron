#pragma once

#include <vector>
#include <optional>
#include <memory>

#include "RDecl.h"
#include "RTypeDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "REnumElemDecl.h"
#include "RTypeDeclOuter.h"

namespace Citron
{

class REnumDecl
    : public RDecl
    , public RTypeDecl
{
    RTypeDeclOuterPtr outer;
    RAccessor accessor;

    RName name;
    std::vector<std::string> typeParams;

    std::optional<std::vector<std::shared_ptr<REnumElemDecl>>> elems; // lazy initialization

    // std::unordered_map<std::string, int> elemsByName;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}

