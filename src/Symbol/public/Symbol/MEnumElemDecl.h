#pragma once

#include <optional>
#include <vector>
#include <memory>

#include "MDecl.h"
#include "MTypeDecl.h"
#include "MEnumElemVarDecl.h"

namespace Citron
{

class MEnumDecl;

class MEnumElemDecl
    : public MDecl
    , public MTypeDecl
{
    std::weak_ptr<MEnumDecl> _enum;
    std::string name;
    std::optional<std::vector<MEnumElemVarDecl>> vars; // lazy-init

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}