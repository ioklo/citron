module;

#include "SymbolConfig.h"
#include <optional>
#include <vector>
#include <memory>
#include <string>

export module Citron.MDecls:MEnumElemDecl;

import :MDecl;
import :MTypeDecl;

namespace Citron
{
export class MEnumElemDecl
    : public MDecl
    , public MTypeDecl
{
    std::weak_ptr<MEnumDecl> _enum;
    std::string name;
    std::optional<std::vector<MEnumElemVarDecl>> vars; // lazy-init

public:
    SYMBOL_API ~MEnumElemDecl();
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}