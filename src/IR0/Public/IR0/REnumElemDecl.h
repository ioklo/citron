#pragma once

#include <optional>
#include <vector>
#include <memory>

#include "RDecl.h"
#include "RTypeDecl.h"
#include "REnumElemMemberVarDecl.h"

namespace Citron
{

class REnumDecl;

class REnumElemDecl
    : public RDecl
    , public RTypeDecl
{
    std::weak_ptr<REnumDecl> _enum;
    std::string name;
    std::optional<std::vector<REnumElemMemberVarDecl>> memberVarDecls; // lazy-init

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}