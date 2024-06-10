export module Citron.Symbols:MEnumElemMemberVarDecl;

import <memory>;
import <optional>;
import :MNames;
import :MType;

namespace Citron
{

class MEnumElemDecl;

export class MEnumElemMemberVarDecl
{   
    std::weak_ptr<MEnumElemDecl> outer;
    MName name;

    std::optional<MType> declType; // lazy-init
};


}