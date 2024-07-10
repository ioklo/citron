#pragma once
#include "SymbolMacros.h"

#include "MAccessor.h"
#include "MCommonFuncDeclComponent.h"

namespace Citron
{

class MStructDecl;
class MFuncParameter;

class MStructConstructorDecl : private MCommonFuncDeclComponent
{
    std::weak_ptr<MStructDecl> _struct;
    MAccessor accessor;
    std::vector<MFuncParameter> parameters;
    bool bTrivial;

public:
    // DECLARE_DEFAULTS(MStructConstructorDecl)
};


}