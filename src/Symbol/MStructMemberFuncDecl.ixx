module;
#include "Macros.h"

export module Citron.Symbols:MStructMemberFuncDecl;

import <memory>;

import :MAccessor;
import :MNames;
import :MCommonFuncDeclComponent;

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
    DECLARE_DEFAULTS(MStructMemberFuncDecl)
};

}