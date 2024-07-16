#pragma once

#include <memory>
#include <optional>

#include "MDecl.h"
#include "MNames.h"
#include "MType.h"

namespace Citron
{

class MEnumElemDecl;

class MEnumElemMemberVarDecl
    : public MDecl
{   
    std::weak_ptr<MEnumElemDecl> outer;
    MName name;

    MTypePtr declType; // lazy-init

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}