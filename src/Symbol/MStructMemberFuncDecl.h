#pragma once
#include "SymbolMacros.h"

#include "MAccessor.h"
#include "MNames.h"
#include "MCommonFuncDeclComponent.h"

namespace Citron
{

class MStructDecl;

class MStructMemberFuncDecl : private MCommonFuncDeclComponent
{
    std::weak_ptr<MStructDecl> _struct;
    MAccessor accessor;
    MName name;
    std::vector<std::string> typeParams;
    bool bStatic;
    
public:
    // DECLARE_DEFAULTS(MStructMemberFuncDecl)
};

}