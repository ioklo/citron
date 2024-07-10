#pragma once

#include "MNames.h"
#include "MType.h"

namespace Citron
{

class MEnumElemDecl;

class MEnumElemMemberVarDecl
{   
    std::weak_ptr<MEnumElemDecl> outer;
    MName name;

    std::optional<MType> declType; // lazy-init
};


}