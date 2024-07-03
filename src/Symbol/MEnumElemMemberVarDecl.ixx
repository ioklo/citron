export module Citron.Symbols:MEnumElemMemberVarDecl;

import "std.h";

import :MNames;
import :MType;

namespace Citron
{

export class MEnumElemDecl;

export class MEnumElemMemberVarDecl
{   
    std::weak_ptr<MEnumElemDecl> outer;
    MName name;

    std::optional<MType> declType; // lazy-init
};


}