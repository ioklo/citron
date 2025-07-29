#pragma once

#include <memory>
#include <optional>

#include "MDecl.h"
#include "MNames.h"

namespace Citron {

class MType;
using MTypePtr = std::shared_ptr<MType>;

class MEnumElemVarDecl
    : public MDecl
{
    std::weak_ptr<MEnumElemDecl> outer;
    MName name;

    MTypePtr declType; // lazy-init

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}