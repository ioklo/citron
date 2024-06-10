export module Citron.Symbols:MEnumElemDecl;

import <memory>;
import <string>;
import <optional>;
import <vector>;

import :MEnumElemMemberVarDecl;

namespace Citron
{

class MEnumDecl;

export class MEnumElemDecl
{
    std::weak_ptr<MEnumDecl> _enum;
    std::string name;
    std::optional<std::vector<MEnumElemMemberVarDecl>> memberVarDecls; // lazy-init
};

}