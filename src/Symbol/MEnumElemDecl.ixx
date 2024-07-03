export module Citron.Symbols:MEnumElemDecl;

import "std.h";

import :MEnumElemMemberVarDecl;

namespace Citron
{

export class MEnumDecl;

export class MEnumElemDecl
{
    std::weak_ptr<MEnumDecl> _enum;
    std::string name;
    std::optional<std::vector<MEnumElemMemberVarDecl>> memberVarDecls; // lazy-init
};

}