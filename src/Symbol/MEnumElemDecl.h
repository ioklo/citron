#pragma once

#include "MEnumElemMemberVarDecl.h"

namespace Citron
{

class MEnumDecl;

class MEnumElemDecl
{
    std::weak_ptr<MEnumDecl> _enum;
    std::string name;
    std::optional<std::vector<MEnumElemMemberVarDecl>> memberVarDecls; // lazy-init
};

}