#pragma once

#include "MAccessor.h"
#include "MType.h"
#include "MNames.h"

namespace Citron
{

class MStructDecl;

class MStructMemberVarDecl
{
    std::weak_ptr<MStructDecl> _struct;

    MAccessor accessor;
    bool bStatic;
    MType declType;
    MName name;
};

}