#pragma once

#include "SymbolConfig.h"
#include <optional>
#include <vector>
#include <string>

#include "MDecl.h"
#include "MTypeDecl.h"

namespace Citron
{
class MEnumElemDecl
    : public MDecl
    , public MTypeDecl
{
    MEnumDecl* _enum;
    std::string name;
    std::optional<std::vector<MEnumElemVarDecl>> vars; // lazy-init

public:
    SYMBOL_API ~MEnumElemDecl();
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}