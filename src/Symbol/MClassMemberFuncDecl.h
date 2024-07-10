#pragma once

#include "MAccessor.h"
#include "MNames.h"
#include "MCommonFuncDeclComponent.h"

namespace Citron
{

class MClassDecl;

class MClassMemberFuncDecl : private MCommonFuncDeclComponent
{
    std::weak_ptr<MClassDecl> _class;
    MAccessor accessor;
    MName name;
    std::vector<std::string> typeParams;
    bool bStatic;
};

}