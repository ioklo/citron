#pragma once
#include "SymbolMacros.h"

#include "MAccessor.h"
#include "MCommonFuncDeclComponent.h"

namespace Citron
{

class MClassDecl;
class MFuncParameter;

class MClassConstructorDecl : private MCommonFuncDeclComponent
{
    std::weak_ptr<MClassDecl> _class;
    MAccessor accessor;
    std::vector<MFuncParameter> parameters;
    bool bTrivial;
};


}