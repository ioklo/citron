#pragma once

#include "MAccessor.h"
#include "MType.h"
#include "MNames.h"

namespace Citron
{

class MClassDecl;

class MClassMemberVarDecl
{
    std::weak_ptr<MClassDecl> _class;

    MAccessor accessor;
    bool bStatic;
    MType declType;
    MName name;
};

}