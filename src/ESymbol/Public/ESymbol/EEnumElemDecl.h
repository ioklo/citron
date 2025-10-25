#pragma once

#include "ESymbolConfig.h"
#include <optional>
#include <vector>
#include <string>

#include "EDecl.h"
#include "ETypeDecl.h"

namespace Citron
{
class EEnumElemDecl
    : public EDecl
    , public ETypeDecl
{
    EEnumDecl* _enum;
    std::string name;
    std::optional<std::vector<EEnumElemVarDecl>> vars; // lazy-init

public:
    ESYMBOL_API ~EEnumElemDecl();
    void Accept(EDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(ETypeDeclVisitor& visitor) override { visitor.Visit(this); }
};

}