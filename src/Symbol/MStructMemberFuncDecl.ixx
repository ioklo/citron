module;
#include "Macros.h"

export module Citron.Symbols:MStructMemberFuncDecl;

import "std.h";

import :MAccessor;
import :MNames;
import :MCommonFuncDeclComponent;

namespace Citron
{

export class MStructDecl;

export class MStructMemberFuncDecl : private MCommonFuncDeclComponent
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